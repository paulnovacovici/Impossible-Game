using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeuralNet;

public class AICtrl : MonoBehaviour {
    public static int numInps;
    public static int numHL = 4;

    InputNode[] inps;
    public float[][] attIniData;
    public string[] attIniString;
    public Vector2 attempt;
    public List<AIPlayer> allPatt = new List<AIPlayer>();
    public List<AIPlayer> startPatt = new List<AIPlayer>();
    public NN highestFit = new NN();
    public string highestFitBrain;
    public GameObject player, start;
    ChallengeController myChallengeController;

    List<float> fits = new List<float>();
    int day;
    float mutationRate = 1f;
    float mutationProb = .05f;

    public enum SelMode { Percent, Top2 };
    public SelMode selMode;

    // Use this for initialization
    void Start () {
        inps = GameObject.FindObjectsOfType<InputNode>();
        numInps = inps.Length;
        myChallengeController = GameObject.FindObjectOfType<ChallengeController>();
        attIniData = new float[(int)attempt.y][];
        attIniString = new string[(int)attempt.y];
        
        for (int i = 0; i < attIniData.Length; i++)
        {
            // Number of weights plus biases
            // inps(HL) + 2*HL + O
            attIniData[i] = new float[numInps * numHL + 2 *numHL + 1];
        }

        RandomizeWholeDay();

        NewDay();
    }
	
	// Update is called once per frame
	void Update () {
    }

    /// <summary>
    /// This starts a new day
    /// </summary>
    public void NewDay()
    {
        // If not day 0
        if (day > 0)
        {
            SetHighest();
            
            LearnFromAttempts();
        }

        // 
        if (day > 0)
        {
            RespawnPlayer((int)attempt.y);
        }
        else
        {
            SpawnPlayer((int)attempt.y);
        }

        // Increase the day by 1
        day++;
        Days.days++;
    }

    public void SpawnPlayer(int num)
    {
        for (int i = 0; i < num; i++)
        {
            AIPlayer p = Instantiate(player).GetComponent<AIPlayer>();

            // Set the position to be that of the starting sphere
            p.transform.position = new Vector3(start.transform.position.x, p.transform.position.y, start.transform.position.z);

            p.SetBrain(attIniData[i]);

            allPatt.Add(p);
        }

        // Need to clear & make way for a new day
        startPatt.Clear();

        // This will copy that list for later use
        for (int i = 0; i < allPatt.Count; i++)
        {
            startPatt.Add(allPatt[i]);
        }
    }

    public void RespawnPlayer(int num)
    {
        GameObject[] grabPast = GameObject.FindGameObjectsWithTag("Passive");

        foreach(InputNode inp in inps)
        {
            inp.Reset();
        }

        for (int i = 0; i < num; i++)
        {
            AIPlayer p = grabPast[i].GetComponent<AIPlayer>();

            p.transform.position = new Vector3(start.transform.position.x, start.transform.position.y, start.transform.position.z);

            p.SetBrain(attIniData[i]);

            // Reset challenges
            myChallengeController.Reset();
            
            // Reset Player
            p.Reset();

            // Reset Score
            GameObject.FindObjectOfType<ScoreScript>().Reset();

            allPatt.Add(p);
        }

        startPatt.Clear();

        for (int i = 0; i < allPatt.Count; i++)
        {
            startPatt.Add(allPatt[i]);
        }
    }

    public void SetHighest()
    {
        AIPlayer tP = startPatt[ReturnHighest(startPatt)];

        // Record highest fitness averages per day
        fits.Add(tP.fitness);

        if (tP.fitness > highestFit.fitness)
        {
            highestFit = new NN(tP.nn.inputs, tP.nn.hL);

            highestFit.SetFitness(tP.fitness);

            highestFit.IniWeights(tP.nn.GetBrain());

            highestFitBrain = highestFit.ReadBrain();
        }
    }

    /// <summary>
    /// This is where Player will pair up his best attempts & learn from them, hopefully
    /// </summary>
    void LearnFromAttempts()
    {
        // First get a reference to the gameobject of all past attempts
        GameObject[] pastAttempts = GameObject.FindGameObjectsWithTag("Passive");

        // Now create a new list to store them all in
        List<NN> allNN = new List<NN>();

        // Next loop through those past attempts, grab a ref to their NN & add them into this linq list of all NNs
        for (int i = 0; i < pastAttempts.Length; i++)
        {
            allNN.Add(pastAttempts[i].GetComponent<AIPlayer>().nn);
        }

        SliceCrossOver(allNN);
    }

    /// <summary>
    /// Makes a slice in one of the parents & inserts the other parent for cross over
    /// </summary>
    /// <param name="parents"></param>
    void SliceCrossOver(List<NN> aNN)
    {
        NN[] parents = new NN[2];

        if (selMode == SelMode.Top2)
        {
            parents = GetFittest2Brains(aNN);
        }

        // Loop through all day attempts & set the attempt ini string to a new string offspring
        for (int i = 0; i < attIniString.Length; i++)
        {
            if (selMode == SelMode.Percent)
            {
                parents = Get2ProbBrains(aNN);
            }

            if (i > attIniString.Length - 3)
            {
                RandomizeAParentWeights(parents);
            }

            // Set each attempt
            attIniString[i] = GenerateOffspringBrain(parents);
        }

        // Set the attempt ini data to the attempt ini string
        for (int i = 0; i < attIniData.Length; i++)
        {
            for (int j = 0; j < attIniData[i].Length; j++)
            {
                attIniData[i][j] = float.Parse(attIniString[i].Split(',')[j]);
            }
        }
    }

    NN[] Get2ProbBrains(List<NN> allNN)
    {
        // set a reference for our fitness sum
        double fitSum = 0;

        // now we need an array to store all the probabilities that are mapped to our allNN list
        double[] allProb = new double[allNN.Count];

        // add all fitness scores to fitness sum
        for (int i = 0; i < allNN.Count; i++)
        {
            fitSum += allNN[i].fitness;
            //print("allNN["+i+"].fitness = " + allNN[i].fitness);
        }

        // now assign our probabilities to our allProb array
        for (int i = 0; i < allProb.Length; i++)
        {
            allProb[i] = allNN[i].fitness / fitSum;
        }

        // now we reorder the array from highest to lowest
        double[] newProb = BubbleSort(false, allProb);

        // create a new array that represents TP range max
        double[] ranges = new double[newProb.Length];

        // set the first range to be the first all
        ranges[0] = newProb[0];

        // set the rest of the ranges based on the previous range
        for (int i = 1; i < ranges.Length; i++)
        {
            ranges[i] = newProb[i] + ranges[i - 1];
        }

        // we need 2 NNs to store our parents
        NN[] ps = new NN[2];

        ps[0] = FindProbParent(allNN, fitSum, newProb, ranges);
        ps[1] = FindProbParent(allNN, fitSum, newProb, ranges);

        if (day > 3)
        {
            while (ps[1] == ps[0])
            {
                ps[1] = FindProbParent(allNN, fitSum, newProb, ranges);
            }
        }

        //print(ps[0].fitness + " & " + ps[1].fitness);
        return ps;
    }

    double[] BubbleSort(bool low2High, double[] prob)
    {
        int checker = 1;
        while (checker != 0)
        {
            checker = 0;

            if (low2High)
            {
                for (int i = 1; i < prob.Length; i++)
                {
                    if (prob[i - 1] > prob[i])
                    {
                        double temp = prob[i - 1];
                        prob[i - 1] = prob[i];
                        prob[i] = temp;
                        checker++;
                    }
                }
            }
            else
            {
                for (int i = 1; i < prob.Length; i++)
                {
                    if (prob[i - 1] < prob[i])
                    {
                        double temp = prob[i - 1];
                        prob[i - 1] = prob[i];
                        prob[i] = temp;
                        checker++;
                    }
                }
            }
        }

        return prob;
    }

    NN FindProbParent(List<NN> allNN, double fitSum, double[] newProb, double[] ranges)
    {
        NN ret = new NN();

        // randomly choose a value in that range
        float choser = Random.value;

        // get a reference to 2 intergers that we need to reverse & remap back to the allNN list for our 2 parents
        int reversal = 0;

        // if the choser value is less than our first range then we will need to reverse the first in the ranges array
        if (choser < ranges[0])
        {
            reversal = 0;
        }

        // now check for if the chooser value falls inbetween the range for any other ranges & if so set the reversal to that
        for (int i = 1; i < ranges.Length; i++)
        {
            if (choser > ranges[i - 1] && choser < ranges[i])
            {
                reversal = i;
                break;
            }
        }

        // 
        for (int i = 0; i < allNN.Count; i++)
        {
            // vvv really good for debugging the whole reversal process vvv
            //print(newProb[reversal] * fitSum + " =? " + allNN[i].fitness + " " + IsApproximatelyEqualTo(newProb[reversal] * fitSum,allNN[i].fitness, .00001f));

            if (IsApproximatelyEqualTo(newProb[reversal] * fitSum, allNN[i].fitness, .00001f))
            {
                ret = allNN[i];
                break;
            }
        }
        return ret;
    }


    bool IsApproximatelyEqualTo(double initialValue, double value, double maximumDifferenceAllowed)
    {
        double a = (initialValue > value) ? initialValue : value;
        double b = (initialValue < value) ? initialValue : value;

        // Handle comparisons of floating point values that may not be exactly the same
        return ((a - b) < maximumDifferenceAllowed);
    }



    /// <summary>
    /// This returns a brain in string form after slice crossing over the parents
    /// </summary>
    /// <param name="parents"></param>
    /// <returns></returns>
    string GenerateOffspringBrain(NN[] parents)
    {
        // First create slice start & stop points
        int start = Random.Range(0, numInps * numHL + 2 * numHL + 1);
        int stop = Random.Range(start, numInps * numHL + 2 * numHL + 1);

        // Then create the offspring brain string
        string offBrain = "";

        // Loop through the first selected parent values all the way until we hit the start of the cut & add that to the offspring's brain
        for (int i = 0; i < start; i++)
        {
            offBrain += parents[0].ReadBrain().Split(',')[i] + ",";
        }

        // Loop through the second selected parent values from the start of the cut to the end of the cut & add that to the offspring's brain
        for (int i = start; i < stop; i++)
        {
            offBrain += parents[1].ReadBrain().Split(',')[i] + ",";
        }

        // Finally loop through the first selected parent values again from the end of the cut to the end of the brain sequence & add that to the offspring's brain
        for (int i = stop; i < numInps * numHL + 2 * numHL + 1; i++)
        {
            // Checks for if at the end of loop or not for comma
            bool com = i != numInps * numHL + 2 * numHL;

            // The adding part
            offBrain += parents[0].ReadBrain().Split(',')[i] + (com ? "," : string.Empty);
        }

        // Last thing we need to to is mutate the offspring's bring using probability so lets create a new offspring brain
        string newOffBrain = "";

        // Then loop through the old brain & using probability mutate the element
        for (int i = 0; i < offBrain.Split(',').Length; i++)
        {
            // Mutation = between 0-1
            float mut = Random.value;

            // if mut < mutation probability then mutate the element
            bool doMut = mut < mutationProb;

            // Checks for if at the end of loop or not for comma
            bool com = i != offBrain.Split(',').Length - 1;

            // The adding part, sorry this is so scary looking :/
            newOffBrain += (float.Parse(offBrain.Split(',')[i]) + ((doMut) ? Random.Range(-mutationRate, mutationRate) : 0)) + (com ? "," : string.Empty);
        }

        // Return the off spring brain
        return newOffBrain;
    }


    /// <summary>
    /// This will randomize a parent's weights at random
    /// </summary>
    /// <param name="parents"></param>
    void RandomizeAParentWeights(NN[] parents)
    {
        // set r that will hold a randomly generated brain
        float[] r = new float[numInps * numHL + 2 * numHL + 1];

        // loop through & generate a brain
        for (int k = 0; k < r.Length; k++)
        {
            r[k] = RandomWeight();
        }

        // coin2 chooses the parent to randomize
        int coin2 = Random.Range(0, parents.Length);

        // randomize the weights for the chosen parent
        parents[coin2].IniWeights(r);
    }

    NN[] GetFittest2Brains(List<NN> allNN)
    {
        // We need 2 new NNs to hold the top 2 performers
        NN[] parents = new NN[2];

        // Set the First Parent to the result of our highest fitness
        parents[0] = allNN[ReturnHighest(allNN)];

        // 
        if (parents[0].fitness > highestFit.fitness)
        {
            highestFit = parents[0];
            //highestFitBrain = highestFit.ReadBrain();
        }

        // Remove that highest performant from the list of all NNs IF there is more than one on the list so that we can run the process again & find the second highest
        if (allNN.Count > 1)
        {
            allNN.Remove(allNN[ReturnHighest(allNN)]);
        }

        // Second parent is chosen
        parents[1] = allNN[ReturnHighest(allNN)];
        return parents;
    }

    /// <summary>
    /// Return the NN with the highest fitness
    /// </summary>
    /// <param name="allNN"></param>
    /// <returns></returns>
    int ReturnHighest(List<AIPlayer> allNN)
    {
        float checker = 0;
        int id = 0;

        // Check for the highest fitness within 
        for (int i = 0; i < allNN.Count; i++)
        {
            id = (allNN[i].fitness > checker) ? i : id;
            checker = (allNN[i].fitness > checker) ? allNN[i].fitness : checker;
        }

        return id;
    }

    /// <summary>
    /// Return the NN with the highest fitness
    /// </summary>
    /// <param name="allNN"></param>
    /// <returns></returns>
    int ReturnHighest(List<NN> allNN)
    {
        double checker = 0;
        int id = 0;

        // Check for the highest fitness within 
        for (int i = 0; i < allNN.Count; i++)
        {
            id = (allNN[i].fitness > checker) ? i : id;
            checker = (allNN[i].fitness > checker) ? allNN[i].fitness : checker;
        }

        return id;
    }

    /// <summary>
    /// This will set each brain configuration for the whole day randomly
    /// </summary>
    void RandomizeWholeDay()
    {
        // set each brain for Data
        for (int i = 0; i < attIniData.Length; i++)
        {
            for (int j = 0; j < attIniData[i].Length; j++)
            {
                attIniData[i][j] = RandomWeight();
            }
        }
    }

    float RandomWeight()
    {
        return Random.Range(-4f, 4f);
    }
}
