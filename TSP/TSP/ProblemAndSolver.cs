using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace TSP
{

    class ProblemAndSolver
    {
        #region Private members 

        /// <summary>
        /// Default number of cities (unused -- to set defaults, change the values in the GUI form)
        /// </summary>
        // (This is no longer used -- to set default values, edit the form directly.  Open Form1.cs,
        // click on the Problem Size text box, go to the Properties window (lower right corner), 
        // and change the "Text" value.)
        private const int DEFAULT_SIZE = 25;

        private const int CITY_ICON_SIZE = 5;

        // For normal and hard modes:
        // hard mode only
        private const double FRACTION_OF_PATHS_TO_REMOVE = 0.20;

        /// <summary>
        /// the cities in the current problem.
        /// </summary>
        private City[] Cities;
        /// <summary>
        /// a route through the current problem, useful as a temporary variable. 
        /// </summary>
        private ArrayList Route;
        /// <summary>
        /// best solution so far. 
        /// </summary>
        private TSPSolution bssf; 

        /// <summary>
        /// how to color various things. 
        /// </summary>
        private Brush cityBrushStartStyle;
        private Brush cityBrushStyle;
        private Pen routePenStyle;


        /// <summary>
        /// keep track of the seed value so that the same sequence of problems can be 
        /// regenerated next time the generator is run. 
        /// </summary>
        private int _seed;
        /// <summary>
        /// number of cities to include in a problem. 
        /// </summary>
        private int _size;

        /// <summary>
        /// Difficulty level
        /// </summary>
        private HardMode.Modes _mode;

        /// <summary>
        /// random number generator. 
        /// </summary>
        private Random rnd;
        #endregion

        #region Public members
        public int Size
        {
            get { return _size; }
        }

        public int Seed
        {
            get { return _seed; }
        }
        #endregion

        #region Constructors
        public ProblemAndSolver()
        {
            this._seed = 1; 
            rnd = new Random(1);
            this._size = DEFAULT_SIZE;

            this.resetData();
        }

        public ProblemAndSolver(int seed)
        {
            this._seed = seed;
            rnd = new Random(seed);
            this._size = DEFAULT_SIZE;

            this.resetData();
        }

        public ProblemAndSolver(int seed, int size)
        {
            this._seed = seed;
            this._size = size;
            rnd = new Random(seed); 
            this.resetData();
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Reset the problem instance.
        /// </summary>
        private void resetData()
        {

            Cities = new City[_size];
            Route = new ArrayList(_size);
            bssf = null;

            if (_mode == HardMode.Modes.Easy)
            {
                for (int i = 0; i < _size; i++)
                    Cities[i] = new City(rnd.NextDouble(), rnd.NextDouble());
            }
            else // Medium and hard
            {
                for (int i = 0; i < _size; i++)
                    Cities[i] = new City(rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble() * City.MAX_ELEVATION);
            }

            HardMode mm = new HardMode(this._mode, this.rnd, Cities);
            if (_mode == HardMode.Modes.Hard)
            {
                int edgesToRemove = (int)(_size * FRACTION_OF_PATHS_TO_REMOVE);
                mm.removePaths(edgesToRemove);
            }
            City.setModeManager(mm);

            cityBrushStyle = new SolidBrush(Color.Black);
            cityBrushStartStyle = new SolidBrush(Color.Red);
            routePenStyle = new Pen(Color.Blue,1);
            routePenStyle.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// make a new problem with the given size.
        /// </summary>
        /// <param name="size">number of cities</param>
        //public void GenerateProblem(int size) // unused
        //{
        //   this.GenerateProblem(size, Modes.Normal);
        //}

        /// <summary>
        /// make a new problem with the given size.
        /// </summary>
        /// <param name="size">number of cities</param>
        public void GenerateProblem(int size, HardMode.Modes mode)
        {
            this._size = size;
            this._mode = mode;
            resetData();
        }

        /// <summary>
        /// return a copy of the cities in this problem. 
        /// </summary>
        /// <returns>array of cities</returns>
        public City[] GetCities()
        {
            City[] retCities = new City[Cities.Length];
            Array.Copy(Cities, retCities, Cities.Length);
            return retCities;
        }

        /// <summary>
        /// draw the cities in the problem.  if the bssf member is defined, then
        /// draw that too. 
        /// </summary>
        /// <param name="g">where to draw the stuff</param>
        public void Draw(Graphics g)
        {
            float width  = g.VisibleClipBounds.Width-45F;
            float height = g.VisibleClipBounds.Height-45F;
            Font labelFont = new Font("Arial", 10);

            // Draw lines
            if (bssf != null)
            {
                // make a list of points. 
                Point[] ps = new Point[bssf.Route.Count];
                int index = 0;
                foreach (City c in bssf.Route)
                {
                    if (index < bssf.Route.Count -1)
                        g.DrawString(" " + index +"("+c.costToGetTo(bssf.Route[index+1]as City)+")", labelFont, cityBrushStartStyle, new PointF((float)c.X * width + 3F, (float)c.Y * height));
                    else 
                        g.DrawString(" " + index +"("+c.costToGetTo(bssf.Route[0]as City)+")", labelFont, cityBrushStartStyle, new PointF((float)c.X * width + 3F, (float)c.Y * height));
                    ps[index++] = new Point((int)(c.X * width) + CITY_ICON_SIZE / 2, (int)(c.Y * height) + CITY_ICON_SIZE / 2);
                }

                if (ps.Length > 0)
                {
                    g.DrawLines(routePenStyle, ps);
                    g.FillEllipse(cityBrushStartStyle, (float)Cities[0].X * width - 1, (float)Cities[0].Y * height - 1, CITY_ICON_SIZE + 2, CITY_ICON_SIZE + 2);
                }

                // draw the last line. 
                g.DrawLine(routePenStyle, ps[0], ps[ps.Length - 1]);
            }

            // Draw city dots
            foreach (City c in Cities)
            {
                g.FillEllipse(cityBrushStyle, (float)c.X * width, (float)c.Y * height, CITY_ICON_SIZE, CITY_ICON_SIZE);
            }

        }

        /// <summary>
        ///  return the cost of the best solution so far. 
        /// </summary>
        /// <returns></returns>
        public double costOfBssf ()
        {
            if (bssf != null)
                return (bssf.costOfRoute());
            else
                return -1D; 
        }

        /// <summary>
        ///  solve the problem.  This is the entry point for the solver when the run button is clicked
        /// right now it just picks a simple solution. 
        /// </summary>
        public void solveProblem()
        {
            int x;
            Route = new ArrayList(); 
            // this is the trivial solution. 
            for (x = 0; x < Cities.Length; x++)
            {
                Route.Add( Cities[Cities.Length - x -1]);
            }
            // call this the best solution so far.  bssf is the route that will be drawn by the Draw method. 
            bssf = new TSPSolution(Route);
            // update the cost of the tour. 
            Program.MainForm.tbCostOfTour.Text = " " + bssf.costOfRoute();
            // do a refresh. 
            Program.MainForm.Invalidate();
        }

        public void genetic()
        {
            uint timesToRun = 4000;
            uint n = (uint)Cities.Length;
            uint populationSize = 1 + (uint)(Math.Sqrt(1 + 2 * n) / 2);

            List<ArrayList> routes = initializePopulation(n);

            Queue<ArrayList> queue = evaluate(routes);

            for (int cycle = 0; cycle < timesToRun; ++cycle)
            {
                List<ArrayList> parents = selection(queue, populationSize);

                queue.Clear();

                crossover(parents, queue);

                compare();
            }
        }

        private List<ArrayList> initializePopulation(uint problemSize)
        {
            //something like this?
            //List<ArrayList> population = new List<ArrayList>();
            //for (int i = 0; i < problemSize; i++)
            //{
            //    population.Add(getRandomRoute().Route);
            //}
            //return population;

            return new List<ArrayList>();
        }

        private Queue<ArrayList> evaluate(List<ArrayList> routes)
        {
            return new Queue<ArrayList>();
        }

        private List<ArrayList> selection(Queue<ArrayList> queue, uint populationSize)
        {
            return new List<ArrayList>();
        }

        private void crossover(List<ArrayList> parents, Queue<ArrayList> queue)
        {
            Random rand = new Random();
            //loop through each combination of parents
            for (int i = 0; i < parents.Count - 1; i++)
            {
                for (int j = i + 1; j < parents.Count; j++)
                {
                    //cross i and j
                    //random number that splits at least one city for crossover
                    int split = rand.Next(Cities.Length - 2) + 1;

                    ArrayList child1 = new ArrayList(parents[i].GetRange(0, split));
                    child1.AddRange(parents[j].GetRange(split, Cities.Length - split));
                    queue.Enqueue(child1);

                    ArrayList child2 = new ArrayList(parents[j].GetRange(0, split));
                    child2.AddRange(parents[i].GetRange(split, Cities.Length - split));
                    queue.Enqueue(child2);
                }
            }

            mutate();
        }

        private void mutate()
        {
        }

        private void compare()
        {
        }

        public void greedy()
        {
            List<Node> nodes = new List<Node>();
            foreach (City city in Cities)
            {
                nodes.Add(new Node(city));
            }

            Route = new ArrayList();
            Node n = nodes[0];

            while (Route.Count < Cities.Length)
            {
                Route.Add(n.getCity());
                n.visit();
                double max = Double.MaxValue;
                Node temp = null;
                foreach (Node node in nodes)
                {
                    if (!node.isVisited() && n.getCity().costToGetTo(node.getCity()) < max)
                    {
                        temp = node;
                        max = n.getCity().costToGetTo(node.getCity());
                    }
                }
                n = temp;
            }

            // call this the best solution so far.  bssf is the route that will be drawn by the Draw method. 
            bssf = new TSPSolution(Route);
            // update the cost of the tour. 
            Program.MainForm.tbCostOfTour.Text = " " + bssf.costOfRoute();
            // do a refresh. 
            Program.MainForm.Invalidate();
        }

        public void random()
        {
            //List<Node> nodes = new List<Node>();
            //foreach (City city in Cities)
            //{
            //    nodes.Add(new Node(city));
            //}

            //Route = new ArrayList();
            //Node n = nodes[0];

            //nodes.Remove(n);
            //Route.Add(n.getCity());
            //bool invalid = true;
            //while (invalid)
            //{
            //    while (Route.Count < Cities.Length)
            //    {
            //        n.visit();
            //        int i = new Random().Next(nodes.Count);
            //        n = nodes[i];
            //        Route.Add(n.getCity());
            //        nodes.Remove(n);
            //    }
            //    // call this the best solution so far.  bssf is the route that will be drawn by the Draw method. 
            //    bssf = new TSPSolution(Route);
            //    if (bssf.costOfRoute() < Double.MaxValue)
            //    {
            //        invalid = false;
            //    }
            //    else
            //    {
            //        nodes = new List<Node>();
            //        foreach (City city in Cities)
            //        {
            //            nodes.Add(new Node(city));
            //        }
            //        Route = new ArrayList();
            //        Route.Add(n.getCity());
            //        n = nodes[0];
            //        nodes.Remove(n);
            //    }
            //}

            bssf = getRandomRoute();

            // update the cost of the tour. 
            Program.MainForm.tbCostOfTour.Text = " " + bssf.costOfRoute();
            // do a refresh. 
            Program.MainForm.Invalidate();
        }

        //I'm thinking we can call this in the random method and in initializePopulation to avoid code duplication
        private TSPSolution getRandomRoute()
        {
            TSPSolution randomRoute;
            List<Node> nodes;
            Node n;
            bool invalid = true;
            do
            {
                nodes = new List<Node>();
                foreach (City city in Cities)
                {
                    nodes.Add(new Node(city));
                }
                Route = new ArrayList();
                n = nodes[0];
                Route.Add(n.getCity());
                nodes.Remove(n);
                while (Route.Count < Cities.Length)
                {
                    n.visit();
                    int i = new Random().Next(nodes.Count);
                    n = nodes[i];
                    Route.Add(n.getCity());
                    nodes.Remove(n);
                }
                randomRoute = new TSPSolution(Route);
                if (randomRoute.costOfRoute() < Double.MaxValue)
                {
                    invalid = false;
                }
            } while (invalid);
            return randomRoute;
        }

        #endregion

        public class Node
        {
            bool visited;
            City city;

            public Node(City c)
            {
                visited = false;
                city = c;
            }

            public bool isVisited()
            {
                return visited;
            }

            public void visit()
            {
                visited = true;
            }

            public City getCity()
            {
                return city;
            }
        }
    }

}
