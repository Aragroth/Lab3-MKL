using System.Runtime.InteropServices;

using Lab2;

struct SplineDataItem
{
    double x { get; set; }
    double yGiven { get; set; }
    double ySpline { get; set; }

    public SplineDataItem(double xStart, double yG, double yS) {
        x = xStart;
        yGiven = yG;
        ySpline = yS;
    }

    public string ToString(string format)
    {
        return $"X: {x.ToString(format)}, yGiven: {yGiven.ToString(format)}, ySpline: {ySpline.ToString(format)}\n";
    }
    public override string ToString()
    {
        return this.ToString("%2");
    }
}


class SplineData
{
    V1DataArray Array { get; set;  }
    int Mvalue { get; set; }
    // double[] InitialY { get; set; } Не нужно, потому что содержится в Array?
    int MaxIterations { get; set; }
    int IterationsMade { get; set; }
    int StopReason {  get; set; }
    double MinValue { get; set; }
    List <SplineDataItem> Data { get; set; }
    int N { get; set; }

    public SplineData(V1DataArray arrayData, int m, int iterationsMax)
    {
        Array = arrayData;
        Mvalue = m;
        MaxIterations = iterationsMax;
        MinValue = Double.PositiveInfinity;
        N = Array!.X.Length;
        IterationsMade = 0;

        Data = new List<SplineDataItem>();
    }

    public void approximize()
    {
        double[] results = new double[N];
        double minNevazka = 0.0;
        int iters = 0;

        try {
            StopReason = TrustRegion(
                Mvalue, Array!.X[0], Array!.X[N - 1], MaxIterations, N, Array!.X, Array!.Y[0],
                ref minNevazka, results, ref iters
            );

            MinValue = minNevazka;
            IterationsMade = iters;
        }
        catch (Exception ex) { 
            Console.WriteLine($"{ex.Message}, RCI_Request stop code: {StopReason}");
            throw new Exception(ex.Message);
        }

        Data.Clear();
        for (int i = 0; i < N; i++) {
            Data.Add(new SplineDataItem(
                Array!.X[i],
                Array!.Y[0][i],
                results[i]
             ));
        }
    }

    public string ToLongString(string format)
    {
        string result = $"{Array.ToLongString(format)}\n";

        for (int i = 0; i < N; i++)
        {
            result += $"{Data[i].ToString(format)}";
        }
        result += $"\nМинимальная невязка: {MinValue}\n";
        result += $"Код остановки RCI_REQUEST: {StopReason}\n";
        result += $"Количество сделанных итераций: {IterationsMade}\n";

        return result;
    }

    [DllImport("\\..\\..\\..\\..\\x64\\Debug\\LibraryMKL.dll",
    CallingConvention = CallingConvention.Cdecl)]
    private static extern
    int TrustRegion(int m, double leftValue, double rightValue,
        int iterMax, int nKnown, double[] KnownX, double[] KnownY, ref double minimized, double[] resultsY, ref int itersMade);
}

internal class Program
{
    static void Main(string[] args)
    {
        int n = 3, m = 2, iterationsMax = 1000;
        V1DataArray dataArray = new(
                $"Array_Data", DateTime.Now, n, 1, 5,
            delegate (double x, ref double y1, ref double y2) { y1 = Math.Pow(x, 3) + Math.Pow(double.E, x); y2 = x * x * x; }
        );

        SplineData spliner = new SplineData(dataArray, m, iterationsMax);
        spliner.approximize();
        Console.WriteLine(spliner.ToLongString("F2"));
    }

    
}

