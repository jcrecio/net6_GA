// Genetic Algorithm implementation in .NET 6 to obtain the optimal route among a set cities


// List of available cities
var cities = new List<City>
{
    new City {Index = 1, Name = "Amenuka", Position = new Point{ X = 23, Y = 104}},
    new City {Index = 2, Name = "Adamantium", Position = new Point{ X = 102, Y = 54}},
    new City {Index = 3, Name = "Stoiker", Position = new Point{ X = 6, Y = 14}},
    new City {Index = 4, Name = "Alexandria", Position = new Point{ X = 92, Y = 91}},
    new City {Index = 5, Name = "Atlantis", Position = new Point{ X = 54, Y = 7}},
    new City {Index = 6, Name = "Nerbia", Position = new Point{ X = 51, Y = 26}},
    new City {Index = 7, Name = "Sostronia", Position = new Point{ X = 29, Y = 88}},
    new City {Index = 8, Name = "Qutan", Position = new Point{ X = 63, Y = 500}},
    new City {Index = 9, Name = "Perkoban", Position = new Point{ X = 203, Y = 73}},
    new City {Index = 10, Name = "Xoner", Position = new Point{ X = 3, Y = 100}},
    new City {Index = 11, Name = "Clastik", Position = new Point{ X = 37, Y = 14}}
};

/// <summary>
/// Get the distance between 2 cities
/// </summary>
double CalculateDistance(City cityA, City cityB)
{
    return Math.Sqrt(
        (Math.Pow(cityA.Position.X - cityB.Position.X, 2) 
        + Math.Pow(cityA.Position.Y - cityB.Position.Y, 2)));
}

/// <summary>
/// Create a cromosome of random gens (sequence of cities randomly) to include in startup
/// </summary>
int[] CreateCromosome() => CreateRandomIntegerArray(11);

/// <summary>
/// Random array of integers of length N
/// </summary>
int[] CreateRandomIntegerArray(int length)
{
    var array = Enumerable.Range(0, length).ToArray();
    var random = new Random();
    int n = array.Length;
    while (n > 1)
    {
        int k = random.Next(n--);
        int temp = array[n];
        array[n] = array[k];
        array[k] = temp;
    }

    return array;
}

/// <summary>
/// Selects the fittest and the worst among a subset of randomly selected parents
/// </summary>
(int[], int[]) SelectCandidates(int[][] population, int initialParentSelection, int fittestSelection)
{
    var randomParentIndexes = CreateRandomIntegerArray(initialParentSelection);
    var fittest = CalculateFitnessForIndividuals(randomParentIndexes, population);
    var (indexesFittest, indexWorst) = GetIndexesForHigherAndLowerValues(
        randomParentIndexes.Select((r, i) => (r, fittest[i])),
        fittestSelection);

    return (indexesFittest, indexWorst);
}

/// <summary>
/// It cross overs pairs of parents to get the new children
/// </summary>
int[][] CrossOver(int[][] population, int[] candidates)
{
    var crossOverPoint = 4; //TODO

    List<int[]> children = new(population.Length/2);
    for (int i = 0; i < candidates.Length; i += 2)
    {
        var parent1PartialGenes = 
            SubArray(population[candidates[i]], 0, crossOverPoint);
        var parent2PartialGenes = 
            SubArray(population[candidates[i]], crossOverPoint, population[candidates[i]].Length - crossOverPoint);
        children.Add(parent1PartialGenes.Concat(parent2PartialGenes).ToArray());
    }

    return children.ToArray();
}

T[] SubArray<T>(T[] array, int offset, int length)
{
    T[] result = new T[length];
    Array.Copy(array, offset, result, 0, length);
    return result;
}

/// <summary>
/// Runs the GA to get a solution for the cities route
/// </summary>
(double,int) RunGeneticAlgorithm(int[][] population, int initialParentSelection, int fittestSelection)
{
    double fittestHistorical = int.MaxValue;
    var generation = 0;
    while(generation < 10)
    {
        var (fittest, worst) = SelectCandidates(population, initialParentSelection, fittestSelection);
        var fit = CalculateFitness(population[fittest[0]]);
        if (fit < fittestHistorical)
        {
            fittestHistorical = fit;
        }
        var children = CrossOver(population, fittest);
        Mutate(children);
        Replace(population, children, worst);

        generation++;
    }

    return GetBestPossibleSolution(population);
}

void Replace(int[][] population, int[][] children, int[] worst)
{
    for (int i = 0; i < children.Length; i++)
    {
        population[worst[i]] = children[i];
    }
}

/// <summary>
/// It mutates an array of cromosomes
/// </summary>
void Mutate(int[][] children)
{
    // probability of cromosome mutation p=0.3
    for (int i = 0; i < children.Length; i++)
    {
        var p = new Random().Next(0, 10);
        if (p <= 3)
        {
            var positionToExchangeA = new Random().Next(0, children.Length);
            var positionToExchangeB = new Random().Next(0, children.Length);
            (children[i][positionToExchangeA], children[i][positionToExchangeB]) =
                (children[i][positionToExchangeB], children[i][positionToExchangeA]);
        }
    }
}

/// <summary>
/// Returns the index of the higher and lower value elements (Elements in shape <index, value>)
/// </summary>
(int[], int[]) GetIndexesForHigherAndLowerValues(IEnumerable<(int, double)> elements, int fittestSelection)
{
    var higherValues = elements
        .OrderByDescending(o => o.Item2)
        .Select(o => o.Item1)
        .Take(fittestSelection)
        .ToArray();
    var lowerValues = elements
        .OrderBy(o => o.Item2)
        .Select(o => o.Item1)
        .Take(fittestSelection)
        .ToArray();
    return (higherValues, lowerValues);
}

/// <summary>
/// 
/// </summary>
double[] CalculateFitnessForIndividuals(int[] indexes, int[][] population)
{
    var result = new double[indexes.Length];
    for (int i = 0; i < indexes.Length; i++)
    {
        result[i] = CalculateFitness(population[indexes[i]]);
    }
    return result;
}

/// <summary>
/// best solution
/// </summary>
(double,int) GetBestPossibleSolution(int[][] population)
{
    var index = -1;
    double bestResult = Int32.MaxValue;
    for (int i = 0; i < population.Length; i++)
    {
        var fitness = CalculateFitness(population[i]);
        if (fitness < bestResult)
        {
            bestResult = fitness;
            index = i;
        }
    }
    return (bestResult, index);
}

/// <summary>
/// Evaluation fitness function for a cromosome
/// </summary>
double CalculateFitness(int[] cromosome)
{
    double totalDistance = 0f;
    for (int i = 1; i < cromosome.Length - 2; i++)
    {
        totalDistance += CalculateDistance(cities[cromosome[i]], cities[cromosome[i + 1]]);
    }
    return totalDistance;
}

/// <summary>
/// Provide a population randomly
/// </summary>
var populationLength = new Random(DateTime.Now.Millisecond).Next(0, 100);
int[][] initialPopulation = Enumerable
    .Range(0, populationLength)
    .Select(p => CreateCromosome())
    .ToArray();

var (solution, cromosome) = RunGeneticAlgorithm(initialPopulation, initialPopulation.Length / 2, 10);

Console.WriteLine($"Optimal distance={solution}");
Console.ReadLine();