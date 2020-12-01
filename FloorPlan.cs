using System;
using System.Collections.Generic;
using System.Linq;

namespace Amazoom
{
    class FloorPlan
    {
        private static List<Vertex> adjList;

        public FloorPlan()
        {
            adjList = new List<Vertex>();
        }

        public int numberOfVertex()
        {
            return adjList.Count;
        }

        public List<Vertex> getAdjList()
        {
            return adjList;
        }
        
        public bool addVertex(string name, bool shelfLeft, bool shelfRight, int numberShelves)
        {
            Vertex newVertex = new Vertex(name, shelfLeft, shelfRight, numberShelves);

            if (adjList.Contains(newVertex))
            {
                return false;
            }
            else
            {
                adjList.Add(newVertex);
                return true;
            }

        }

        //could use a beter implementation than foreach?
        public bool addEdge(string vertex1, string vertex2)
        {
            var matches1 = adjList.Any(p => p.getName() == vertex1);

            var matches2 = adjList.Any(p => p.getName() == vertex2);

            if (matches1 && matches2)
            {
                foreach (Vertex vertex in adjList)
                {
                    if (vertex.getName() == vertex1)
                    {
                        vertex.addUndirectedEdge(vertex2);
                    }
                    else if (vertex.getName() == vertex2)
                    {
                        vertex.addUndirectedEdge(vertex1);
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        //could use a better implementation than foreach?
        public bool removeEdge(string vertex1, string vertex2)
        {
            var matches1 = adjList.Any(p => p.getName() == vertex1);

            var matches2 = adjList.Any(p => p.getName() == vertex2);

            if (matches1 && matches2)
            {
                foreach (Vertex vertex in adjList)
                {
                    if (vertex.getName() == vertex1)
                    {
                        vertex.removeUndirectedEdge(vertex2);
                    }
                    else if (vertex.getName() == vertex2)
                    {
                        vertex.removeUndirectedEdge(vertex1);
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }
        //TODO: return all paths and alternate paths, move to central computer
        public List<string> getRoute(string start, string end)
        {

            List<string> path = new List<string>();

            if ( start == end)
            {
                path.Add(end);
                return path;
            }

            foreach(Vertex vertex in adjList)
            {
                vertex.distance = int.MaxValue;
                vertex.visited = false;
                vertex.predecessor = "null";
            }

            if(BFS(start, end) == false)
            {
                Console.WriteLine("No path is available.");
                return null;
            }

            //for testing
            /*
            foreach(Vertex vertex in adjList)
            {
                Console.WriteLine(vertex.getName() + " " + vertex.distance.ToString());
            }*/

            //List<string> path = new List<string>();
            path.Add(end);

            string predecessor = "";

            int index = adjList.FindIndex(obj => obj.getName() == end);

            while (predecessor != "null")
            {
                predecessor = adjList[index].predecessor;

                if(predecessor == "null")
                {
                    break;
                }

                path.Add(predecessor);
                index = adjList.FindIndex(obj => obj.getName() == predecessor);

            }

            Console.Write("Path: ");

            for (int i = path.Count - 1; i >= 0; i--)
            {
                Console.Write(path[i] + " ");
            }

            Console.WriteLine();

            return path;
        }
        //move to central computer class
        private static bool BFS(string start, string end)
        {
            Queue<string> queue = new Queue<string>();

            int startIndex = adjList.FindIndex(obj => obj.getName() == start);

            adjList[startIndex].visited = true;
            adjList[startIndex].distance = 0;

            queue.Enqueue(start);

            while (queue.Count != 0)
            {
                string u = queue.Peek();

                queue.Dequeue();

                int index = adjList.FindIndex(obj => obj.getName() == u);

                List<string> listOfEdges = adjList[index].getUndirectedEdges();

                for ( int i =0; i < listOfEdges.Count; i++)
                {
                    string edge = listOfEdges[i];
                    int index2 = adjList.FindIndex(obj => obj.getName() == edge);
                    if (adjList[index2].visited == false)
                    {
                        adjList[index2].visited = true;
                        adjList[index2].distance = adjList[index].distance + 1;
                        adjList[index2].predecessor = u;

                        queue.Enqueue(edge);

                        if(edge == end)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
    class Vertex
    {
        //vertex name
        private string name;
        //vertex edges
        private List<string> undirectedEdges;
        public bool visited { get; set; }
        public int distance { get; set; }
        public string predecessor { get; set; }

        public readonly bool shelfLeft;

        public readonly bool shelfRight;

        public readonly int numberShelves;

        public Dictionary<string, double> shelfLoad;

        public Vertex(string Name, bool ShelfLeft, bool ShelfRight, int NumberShelves)
        {
            name = Name;
            shelfLeft = ShelfLeft;
            shelfRight = ShelfRight;
            numberShelves = NumberShelves;
            undirectedEdges = new List<string>();
            shelfLoad = new Dictionary<string, double>();

            string shelfName;

            if (shelfLeft)
            {
                for(int i = 0; i < NumberShelves; i++)
                {
                    shelfName = "L" + (i+1).ToString();
                    shelfLoad[shelfName] = 0;
                }
            }
            if (shelfRight)
            {
                for (int j = 0; j < NumberShelves; j++)
                {
                    shelfName = "R" + (j+1).ToString();
                    shelfLoad[shelfName] = 0;
                }
            }
        }

        public string getName()
        {
            return name;
        }

        public void addUndirectedEdge(string vertexName)
        {
            undirectedEdges.Add(vertexName);
        }
        public void removeUndirectedEdge(string vertexName)
        {
            undirectedEdges.Remove(vertexName);
        }

        public List<string> getUndirectedEdges()
        {
            return undirectedEdges;
        }
    }
}
