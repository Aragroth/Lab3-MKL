namespace Lab2
{
   
    public delegate void FValues(double x, ref double y1, ref double y2);
    public delegate DataItem FDI(double x);

    public struct DataItem
    {
        public double X { get; set; }
        public double Y1 { get; set; }
        public double Y2 { get; set; }

        public DataItem(double x, double y1, double y2) { X = x; Y1 = y1; Y2 = y2; }
        public string ToLongString(string format)
        {
            string formattedX = X.ToString(format);
            string formattedY1 = Y1.ToString(format);
            string formattedY2 = Y2.ToString(format);

            return $"X: {formattedX}, Y1: {formattedY1}, Y2: {formattedY2}";
        }

        public override string ToString()
        {
            return this.ToLongString("F3");
        }
    }

    public abstract class V1Data : IEnumerable<DataItem>
    {
        public string Key { get; set; }
        public DateTime Date { get; set; }

        public V1Data(string info, DateTime date)
        {
            Key = info;
            Date = date;
        }

        public abstract double MaxDistance { get; }
        public abstract string ToLongString(string format);

        public override string ToString()
        {
            return ToLongString("F3");
        }

        public abstract IEnumerator<DataItem> GetEnumerator();

        
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    public class V1DataList : V1Data
    {
        public List<DataItem> Data { get; set; }

        public V1DataList(string key, DateTime date) : base(key, date)
        {
            Data = new List<DataItem>();
        }

        public V1DataList(string key, DateTime date, double[] x, FDI F) : this(key, date)
        {
            HashSet<double> uniqueX = new HashSet<double>();

            for (int i = 0; i < x.Length; i++)
            {
                double currentX = x[i];
                if (!uniqueX.Contains(currentX))
                {
                    DataItem item = F(currentX);
                    Data.Add(item);
                    uniqueX.Add(currentX);
                }
            }
        }

        public override double MaxDistance
        {
            get
            {
                if (Data.Count < 2) return 0.0;
                double maxDist = double.MinValue;
                for (int i = 0; i < Data.Count - 1; i++)
                {
                    for (int j = i + 1; j < Data.Count; j++)
                    {
                        double dist = Math.Abs(Data[i].X - Data[j].X);
                        if (dist > maxDist) maxDist = dist;
                    }
                }
                return maxDist;
            }
        }

        public static explicit operator V1DataArray(V1DataList source)
        {
            V1DataArray array = new V1DataArray(source.Key, source.Date, source.Data.Count);
            for (int i = 0; i < source.Data.Count; i++)
            {
                array.X[i] = source.Data[i].X;
                array.Y[0][i] = source.Data[i].Y1;
                array.Y[1][i] = source.Data[i].Y2;
            }
            return array;
        }

        public override string ToString()
        {
            return $"V1DataList: {Key}, Date: {Date} Count: {Data.Count}";
        }

        public override string ToLongString(string format)
        {
            string dataItems = string.Join(Environment.NewLine, Data.Select(
                item => $"{item.ToLongString(format)}"
            ));
            return $"{ToString()}\n{dataItems}";
        }

        public override IEnumerator<DataItem> GetEnumerator()
        {
            foreach (var item in Data)
            {
                yield return item;
            }
        }
    }

    public class V1DataArray : V1Data
    {
        public double[] X { get; set; }
        public double[][] Y { get; set; }

        public V1DataArray(string key, DateTime date, int N) : base(key, date)
        {
            X = new double[N];
            Y = new double[2][];

            Y[0] = new double[N];
            Y[1] = new double[N];
        }

        public V1DataArray(string key, DateTime date) : this(key, date, 0) { }

        public V1DataArray(string key, DateTime date, double[] x, FValues F) : base(key, date)
        {
            X = new double[x.Length];

            Y = new double[2][];
            Y[0] = new double[x.Length];
            Y[1] = new double[x.Length];

            X.CopyTo(x, 0);

            for (int i = 0; i < x.Length; i++)
            {
                F(x[i], ref Y[0][i], ref Y[1][i]);
            }
        }

        public V1DataArray(string key, DateTime date, int nX, double xL, double xR, FValues F) : this(key, date, nX)
        {
            double step = (xR - xL) / (nX - 1);
            for (int i = 0; i < nX; i++)
            {
                X[i] = xL + i * step;
                F(X[i], ref Y[0][i], ref Y[1][i]);
            }
        }

        public double[] this[int index]
        {
            get => Y[index];
        }

        public V1DataList V1DataList
        {
            get
            {
                V1DataList list = new(base.Key, base.Date);
                for (int i = 0; i < X.Length; i++)
                {
                    list.Data.Add(new DataItem(X[i], Y[0][i], Y[1][i]));
                }
                return list;
            }
        }

        public override double MaxDistance
        {
            get
            {
                if (X.Length < 2) return 0.0;

                double maxDist = double.MinValue;
                for (int i = 0; i < X.Length - 1; i++)
                {
                    for (int j = i + 1; j < X.Length; j++)
                    {
                        double dist = Math.Abs(X[i] - X[j]);
                        if (dist > maxDist) maxDist = dist;
                    }
                }
                return maxDist;
            }
        }

        public override string ToString()
        {
            return $"V1DataArray: {Key}, Date: {Date} Count: {X.Length}";
        }

        public override string ToLongString(string format)
        {
            string dataItems = "";
            for (int i = 0; i < X.Length; i++)
                dataItems += $"X: {X[i].ToString(format)}, Y1: {Y[0][i].ToString(format)}, Y2: {Y[1][i].ToString(format)}\n";
            return $"{ToString()}\n{dataItems}";
        }

        public override IEnumerator<DataItem> GetEnumerator()
        {
            for (int i = 0; i < X.Length; i++)
            {
                yield return new DataItem(X[i], Y[0][i], Y[1][i]);
            }
        }


        public bool Save(string filename)
        {
            try
            {
                FileStream fout = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write);
                BinaryWriter bw = new BinaryWriter(fout);

                bw.Write(Key);
                bw.Write(Date.ToBinary());
                bw.Write(X.Length);

                for (int i = 0; i < X.Length; i++)
                {
                    bw.Write(X[i]);
                    bw.Write(Y[0][i]);
                    bw.Write(Y[1][i]);
                }
                fout.Close();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while saving: " + ex.Message);
                return false;
            }
        }


        public static bool Load(string filename, ref V1DataArray dataArray)
        {
            try
            {
                FileStream fout = File.OpenRead(filename);
                BinaryReader bw = new BinaryReader(fout);

                dataArray.Key = bw.ReadString();
                dataArray.Date = DateTime.FromBinary(bw.ReadInt64());
                var length = bw.ReadInt32();

                dataArray.X = new double[length];
                dataArray.Y = new double[2][];
                dataArray.Y[0] = new double[length];
                dataArray.Y[1] = new double[length];

                for (int i = 0; i < length; i++)
                {
                    dataArray.X[i] = bw.ReadDouble();
                    dataArray.Y[0][i] = bw.ReadDouble();
                    dataArray.Y[1][i] = bw.ReadDouble();
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while saving: " + ex.Message);
                return false;
            }
        }
    }
}
