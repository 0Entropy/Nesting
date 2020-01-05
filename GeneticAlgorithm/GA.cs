using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace Nesting.GeneticAlgorithm
{
    using UnityEngine;
    using Random = System.Random;
    //using PDF;
    using Geometry;

    using Polygon = List<UnityEngine.Vector2>;
    using Polygons = List<List<UnityEngine.Vector2>>;

    using Path = List<ClipperLib.IntPoint>;
    using Paths = List<List<ClipperLib.IntPoint>>;

    using ClipperLib;

    public class GA
    {

        static Random rand = new Random();
        /// <summary>
        /// 基因交换概率；
        /// </summary>
        static double crossRate = .7;
        /// <summary>
        /// 基因突变概率；
        /// </summary>
        static double mutRate = .001;
        /// <summary>
        /// 基因池存储量；
        /// </summary>
        static int poolSize = 2;

        static Rect binBoundary;
        static Polygons rawPolys;
        static FPLibrary FPLib;

        public GA(Rect boundary, Polygons polys)
        {
            Console.WriteLine(boundary.ToString());


            GA.FPLib = new FPLibrary(boundary);
            GA.binBoundary = boundary;
            GA.rawPolys = polys;
        }

        //遗传算子，模拟生物进化
        public Chomosone OnProcess(List<int> rawIds)
        {
            List<Chomosone> pool = new List<Chomosone>();//原始库挑选
            List<Chomosone> remain = new List<Chomosone>();//进化后剩下来的

            for (int i = 0; i < poolSize; i++)
                pool.Add(new Chomosone(rawIds));

            float minFitness = float.MaxValue;
            int count = 0;
            Chomosone chomosone = null;
            while (count < 1)
            {
                remain.Clear();

                for (int x = pool.Count - 1; x >= 0; x -= 2)
                {
                    Chomosone c0 = SelectFrom(pool);
                    Chomosone c1 = SelectFrom(pool);

                    c0.CrossOver(c1);

                    c0.Mutate();
                    c1.Mutate();

                    c0.Place();
                    c1.Place();

                    remain.Add(c0);
                    remain.Add(c1);
                }

                pool.AddRange(remain);
                count++;

                //按照适应度大小从小到大排序，从剩余中找出适应度最小的个体
                var best = pool.OrderBy(cho => cho.fitness).First();
                if (best.fitness != 1 && best.fitness < minFitness)
                {
                    minFitness = best.fitness;
                    chomosone = (Chomosone)best.Clone();//每次循环找出最优个体
                }
            }

            Debug.Log("Min fitness : " + minFitness);
            Debug.Log("Chom fitness : " + chomosone.fitness);

            //---返回适应度最小的个体
            return chomosone;
        }

        //遗传算法，选择--从染色体池中选择一个染色体--（天择）
        //找出适应度低于随机数的个体出来----轮盘赌算法        
        private Chomosone SelectFrom(List<Chomosone> pool)
        {
            Chomosone individual = null;

            //Calc the total fitness
            float total = 0f;
            for (int i = pool.Count - 1; i >= 0; i--)
            {
                total += 1.0f / pool[i].fitness; //此处求1除以每个适应因子，再求和
            }
            float slice = (float)(total * rand.NextDouble());

            //Loop to find the individual
            float next = 0f;
            for (int i = pool.Count - 1; i >= 0; i--)
            {
                next += 1.0f / pool[i].fitness;
                if (next >= slice)
                {
                    individual = pool[i];
                    break;
                }
            }
            if (individual == null)
                individual = pool[pool.Count - 1];
            pool.Remove(individual);
            return individual;
        }

        //Chomosone染色体-将搜索空间中的解转换成遗传算法中的染色体或个体
        public class Chomosone : ICloneable
        {
            //genes是一个List集合，集合数量就是pdf拼合的总数，每个元素包含一个pdf的顶点集合信息--基因池
            public List<DNA> genes;

            //适应因子---物种之间的竞争（物竞）
            public float fitness = 1f;

            public Rect Bounds;

            public Chomosone(List<int> rawIDs)
            {
                genes = new List<DNA>(rawIDs.Count);
                foreach (var id in rawIDs)
                    genes.Add(new DNA(id));
            }

            public Chomosone(int count)
            {
                genes = new List<DNA>(count);
                for (int i = 0; i < count; i++)
                    genes.Add(new DNA(i));
            }

            public Chomosone(List<DNA> genes)
            {
                this.genes = genes;
            }

            //摆放位置
            public void Place()
            {
                fitness = 1f;
                List<DNA> remain = new List<DNA>();
                remain.AddRange(genes);

                List<DNA> placed = new List<DNA>();



                for (int i = 0; i < remain.Count; i++)
                {
                    var dna = remain[i];
                    var ifp = FPLib.Lookup(dna);
                    var ifpPath = ifp.Select(v => new IntPoint(v.x, v.y)).ToList();
                    Vector2? pos = null;
                    //The first one;
                    if (i == 0)
                    {
                        var bounds = dna.pattern.Rect();
                        //Debug.Log("rect 0 : " + bounds);
                        pos = bounds.center - bounds.min;
                        //Debug.Log("Pos : " + pos);
                        Debug.Log("dan ID : " + dna.id + ", First Position : " + pos.Value);
                        //var bounds = dna.pattern.Rect();
                        dna.position = pos.Value;
                        placed.Add(dna);
                        continue;
                    }

                    Clipper unionClipper = new Clipper();

                    Paths combinedPath = new Paths();

                    foreach (var place in placed)
                    {
                        var nfp = FPLib.Lookup(new DNAPair(dna, place));
                        if (nfp == null)
                            continue;
                        var path = nfp.Select(v => new IntPoint(v.x + place.position.x, v.y + place.position.y)).ToList();
                        //area >  0.1..... && length > 2.....
                        unionClipper.AddPath(path, PolyType.ptSubject, true);

                    }

                    if (!unionClipper.Execute(ClipType.ctUnion, combinedPath, PolyFillType.pftNonZero, PolyFillType.pftNonZero))
                    {
                        continue;
                    }

                    var finalPath = new Paths();
                    Clipper diffClipper = new Clipper();


                    

                    diffClipper.AddPaths(combinedPath, PolyType.ptClip, true);
                    diffClipper.AddPath(ifpPath, PolyType.ptSubject, true);

                    //ClipType.ctXor共同区域减去公共区域
                    if (!diffClipper.Execute(ClipType.ctDifference, finalPath, PolyFillType.pftNonZero, PolyFillType.pftNonZero))
                    {
                        continue;
                    }

                    var finalNFP = finalPath.Select(path => path.Select(p => new Vector2(p.X, p.Y)).ToList()).ToList();

                    //Debug.Log("final Count : " + finalNFP.Count());

                    //Vector2? vertex = null;

                    float vMin = float.MaxValue;

                    foreach (var poly in finalNFP)
                    {
                        foreach (var vertex in poly)
                        {
                            //vertex = vertex;// - start;
                            var shiftPath = dna.pattern.Select(v => v + vertex).ToList();

                            var allpoints = placed.SelectMany(d => d.path).ToList();
                            allpoints.AddRange(shiftPath);
                            var bounds = allpoints.Rect();

                            var value = Mathf.Sqrt(bounds.width * bounds.height) + vertex.y;

                            if (value < vMin)
                            //if (isMinY)
                            {
                                vMin = value;
                                pos = vertex;
                            }
                        }
                    }

                    if (pos != null)
                    {
                        dna.position = pos.Value;
                        placed.Add(dna);
                        Debug.Log("dan ID : " + dna.id + ", Position : " + pos.Value);
                    }

                }//end of for(int i = 0; i<remain.Count; i++)

                Bounds = placed.SelectMany(d => d.path).Rect();
                //if ( minWidth != null )
                fitness += (Bounds.width * Bounds.height) / (binBoundary.width * binBoundary.height);
                //fitness += minWidth.Value / binBoundary.width;

                if (placed.Count == 0)
                    return;

                foreach (var dna in placed)
                {
                    remain.Remove(dna);//移除已放置的
                }

                fitness += 2 * remain.Count;

                //Debug.Log("Fitness : " + fitness);
            }

            //遗传算法变异操作
            internal void Mutate()
            {
                var chomLength = genes.Count;

                for (int x = 0; x < chomLength; x++)
                {
                    if (rand.NextDouble() < mutRate)
                    {
                        if (rand.NextDouble() > 0.5F)
                        {
                            var nextIndex = rand.Next(chomLength);
                            if (nextIndex == x) nextIndex = (nextIndex + 1) % chomLength;

                            var tmpDNA = genes[x];
                            genes[x] = genes[nextIndex];
                            genes[nextIndex] = tmpDNA;
                        }
                        else
                        {
                            genes[x].Mutate();
                        }
                        //genes[x].Mutate();//变异DNA中的angle

                    }
                }
            }

            //遗传算法交叉操作--交换随机数和基因长度之间的基因
            //genes是一个List<DNA>集合，交换集合中DNA的顺序 
            public void CrossOver(Chomosone other)
            {
                //产生一个随机数，根据随机数大小和突变概率比较，大则发生交叉事件
                if (rand.NextDouble() > crossRate) return;

                var chomLength = genes.Count;
                int pos = rand.Next(chomLength);
                for (int x = pos; x < chomLength; x++)
                {
                    var tmpDNA = genes[x];
                    genes[x] = other.genes[x];
                    other.genes[x] = tmpDNA;
                }
            }

            //防止进化过程中产生的最优解被交叉和变异所破坏，可以将每一代中的最优解原封不动的复制到下一代中
            public object Clone()
            {
                var genesClone = (List<DNA>)genes.Clone<DNA>();
                Chomosone clone = new Chomosone(genesClone);
                clone.fitness = this.fitness;
                clone.Bounds = this.Bounds;
                return clone;
            }
        }

        public class DNAPair : IEquatable<DNAPair>
        {
            public DNA pattern;
            public DNA path;

            public DNAPair(DNA pattern, DNA path)
            {
                this.pattern = pattern;
                this.path = path;
            }

            public Polygon NFP
            {
                get
                {
                    return pattern.NFP(path);
                }
            }

            public bool Equals(DNAPair other)
            {
                if (other == null)
                    return false;

                if (this.pattern == other.pattern && this.path == other.path)
                    return true;
                else
                    return false;
            }

            public override bool Equals(System.Object obj)
            {
                if (obj == null)
                    return false;

                DNAPair other = obj as DNAPair;
                if (other == null)
                    return false;
                else
                    return Equals(other);
            }

            public override int GetHashCode()
            {
                return this.pattern.GetHashCode() ^ this.path.GetHashCode();
            }

            public static bool operator ==(DNAPair pair1, DNAPair pair2)
            {
                if (((object)pair1) == null || ((object)pair2) == null)
                    return Object.Equals(pair1, pair2);

                return pair1.Equals(pair2);
            }

            public static bool operator !=(DNAPair pair1, DNAPair pair2)
            {
                if (((object)pair1) == null || ((object)pair2) == null)
                    return !Object.Equals(pair1, pair2);

                return !(pair1.Equals(pair2));
            }
        }

        public class DNA : IEquatable<DNA>, ICloneable
        {
            public int id;

            public float angle { set; get; }
            public Vector2 position { set; get; }

            public Polygon pattern
            {
                get
                {
                    var _pattern = rawPolys[id].RotatePolygon(angle).ToList();
                    if (GeometryUtils.PolygonArea(_pattern) > 0)
                        _pattern.Reverse();
                    return _pattern;
                }
            }

            public Polygon path { get { return pattern.Select(v => v + position).ToList(); } }

            public DNA(int id)
            {
                this.id = id;
                angle = RandomAngle();
            }

            public DNA() { }

            public void Mutate()
            {
                angle = RandomAngle();
            }

            // "Inner Fit Polygon" 
            public Polygon IFP(ref Rect binBoundary)
            {
                var boundary = this.pattern.Rect();

                if (boundary.width > binBoundary.width)
                {
                    binBoundary.xMax += binBoundary.width;
                    Debug.Log("After Expand xMax : " + binBoundary);
                }
                if (boundary.height > binBoundary.height)
                {
                    binBoundary.yMax += binBoundary.height;
                    Debug.Log("Ater Expand yMax : " + binBoundary);
                }

                var start = this.pattern[0];
                //var delta = start - boundary.min;

                var position = binBoundary.min + boundary.size * 0.5f;
                var size = binBoundary.size - boundary.size;
                var newRect = new Rect(position, size);
                Debug.Log(id + " - IFP Bounds : " + newRect);
                return newRect.Vector2List();

            }

            // "No Fit Polygon"
            // "orbit" this DNA's poly around other DNA's poly 
            public Polygon NFP(DNA other)
            {
                var thisPath = this.pattern.Select(v => new IntPoint(v.x, v.y)).ToList();
                //var start = this.pattern[0];
                var otherPath = other.pattern.Select(v => new IntPoint(v.x, v.y)).ToList();
                otherPath.Reverse();

                //var solution = Clipper.MinkowskiSum(thisPath, otherPath, false);
                var solution = Clipper.MinkowskiDiff(thisPath, otherPath);//, false);
                //var _outmostRect = new Rect();
                Polygon _outmostPolygon = solution[0].Select(p => new Vector2(p.X/* + start.x*/, p.Y/* + start.y*/)).ToList();

                return _outmostPolygon;
            }

            public bool Equals(DNA other)
            {
                if (other == null)
                    return false;

                if (this.id == other.id && this.angle == other.angle)
                    return true;
                else
                    return false;
            }

            public override bool Equals(System.Object obj)
            {
                if (obj == null)
                    return false;

                DNA other = obj as DNA;
                if (other == null)
                    return false;
                else
                    return Equals(other);
            }

            public override int GetHashCode()
            {
                return this.id.GetHashCode() ^ this.angle.GetHashCode();
            }

            public static bool operator ==(DNA dna1, DNA dna2)
            {
                if (((object)dna1) == null || ((object)dna2) == null)
                    return Object.Equals(dna1, dna2);

                return dna1.Equals(dna2);
            }

            public static bool operator !=(DNA dna1, DNA dna2)
            {
                if (((object)dna1) == null || ((object)dna2) == null)
                    return !Object.Equals(dna1, dna2);

                return !(dna1.Equals(dna2));
            }

            public object Clone()
            {
                DNA clone = new DNA(this.id);//, this.index);
                clone.angle = this.angle;
                clone.position = this.position;
                return clone;
            }
        }


        static List<T> Shuffle<T>(List<T> list)
        {
            int currentIndex = list.Count;// array.Length;
            T tempValue;//, randomValue;
            int randomIndex;

            // While there remain elements to shuffle...
            while (0 != currentIndex)
            {
                Random rand = new Random();
                // Pick a remaining element...
                randomIndex = rand.Next(currentIndex);
                currentIndex -= 1;

                // And swap it with the current element.
                tempValue = list[currentIndex];
                list[currentIndex] = list[randomIndex];
                list[randomIndex] = tempValue;
            }

            return list;
        }

        static float RandomAngle()
        {
            // i * ( 360 / config.rotations )
            //return 0f;
            return (Config.initAngle + rand.Next(Config.angleNum) * (360f / Config.angleNum));//%360;
        }

    }
}
