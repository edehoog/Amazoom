using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Amazoom
{
    class Robot
    {
        public string name;
        string position = "A6";
        string home = "A6";
        double batteryLevel = 100; //%
        double batteryCapacity = 1000; //s
        double loadingCapacity = 500; //kg
        bool truckIsDocked = false;
        bool adjacentRobot = false;

        FloorPlan floorPlan;
        Queue<string> pathTo; //way to simplify these all into just one?
        Queue<string> pathHome;
        Queue<string> pathDock;

        List<string> itemsOverload;
        List<string> load;

        List<List<string>> loadPickup;

        Job myJob; //add bolean to see if order or stocking
        Task job;

        static Random rand = new Random();

        public Robot( string Name, FloorPlan newFloorPlan)
        {
            name = Name;
            floorPlan = newFloorPlan;
            pathTo = new Queue<string>();
            pathHome = new Queue<string>();
            pathDock = new Queue<string>();
            itemsOverload = new List<string>();
            loadPickup = new List<List<string>>();
            load = new List<string>();
        }

        public void turnOnRobot()
        {
            job = new Task(() => startRobotJob());
            job.Start();
        }

        public void startRobotJob()
        {
            Console.WriteLine("Task started");
            while (true)
            {
                Thread.Sleep(rand.Next(1000));
                {
                    lock (RobotTaskLock.SyncRoot)
                    {
                        //Console.WriteLine(name + " is waiting for its task.");
                        if (MySynchronizedJobQueue<Job>.Count != 0)
                        {
                            myJob = MySynchronizedJobQueue<Job>.Dequeue();
                            myJob.DequeueDateTime = DateTime.Now;
                            Console.WriteLine(name + " received its task.");
                        }
                        
                    }

                    //there needs to be a delivery truck docked, rn the job is being created with no truck assigned in the bay
                    if(myJob != null)
                    {
                        if (myJob.isOrder && (Dock1.stocking == false || Dock2.stocking == false))
                        {
                            if (Dock1.stocking == false && Dock2.stocking == false)
                            {
                                int dock = rand.Next(2);

                                if(dock == 1)
                                {
                                    myJob.dockNumber = "C5";
                                }
                                else
                                {
                                    myJob.dockNumber = "D5";
                                }
                            }
                            else if(Dock1.stocking == false)
                            {
                                myJob.dockNumber = "C5";
                            }
                            else
                            {
                                myJob.dockNumber = "D5";
                            }

                            double weightCheck = 0;

                            int counter = 1;

                            foreach (KeyValuePair<string, double[]> item in myJob.listOfItems)
                            {
                                Item itemName = CentralComputer.productList.Read(item.Key.ToUpper());

                                weightCheck += itemName.itemWeight * item.Value[0];

                                if (weightCheck > loadingCapacity)
                                {
                                    itemsOverload.Add(item.Key);
                                    weightCheck = 0;
                                    loadPickup.Add(load);
                                    load = new List<string>();

                                    if (counter == myJob.listOfItems.Count)
                                    {
                                        load.Add(item.Key);
                                        loadPickup.Add(load);
                                        load = new List<string>();
                                    }
                                }
                                else
                                {
                                    load.Add(item.Key);

                                    if (counter == myJob.listOfItems.Count)
                                    {
                                        loadPickup.Add(load);
                                    }
                                }
                                counter++;
                            }

                            int count = 0;

                            List<string> route = new List<string>();

                            Console.WriteLine("test" + myJob.dockNumber);

                            foreach (KeyValuePair<string, double[]> item in myJob.listOfItems)
                            {
                                //null even before here
                                Console.WriteLine(myJob.dockNumber);

                                Item itemName = CentralComputer.productList.Read(item.Key.ToUpper());

                                if (itemsOverload.Contains(item.Key))
                                {
                                    //if contains its a signal to go drop off the load at the dock
                                    route = floorPlan.getRoute(position, myJob.dockNumber);
                                    getPathDock(route);
                                    GoTo();

                                    //this needs to only be executed at the dock
                                    Console.WriteLine("count: " + loadPickup[count].Count);

                                    foreach (string items in loadPickup[count])
                                    {
                                        putDownItem(CentralComputer.productList.Read(items));
                                        Console.WriteLine("robot put down the items at the dock");
                                    }
                                    count++;
                                }

                                route = floorPlan.getRoute(position, itemName.vertexLocation);

                                position = itemName.vertexLocation;
                                getPath(route);
                                GoTo();

                                //for the getting delivery version this needs to call execute order
                                executeOrderRetrival(itemName, item.Value[0]); // make sure that the second term is actually the amount ordered
                            }
                            Console.WriteLine("HEREWERWER");

                            Console.WriteLine(position);
                            //dock number is null???
                            Console.WriteLine(myJob.dockNumber);

                            route = floorPlan.getRoute(position, myJob.dockNumber);

                            getPathDock(route);

                            GoTo();
                            foreach (string items in loadPickup[count])
                            {
                                putDownItem(CentralComputer.productList.Read(items));
                                Console.WriteLine("robot put down the items at the dock");
                            }
                            GoHome();
                            loadingCapacity = 500;
                            loadPickup.Clear();
                            load.Clear();

                            //need to realse dock semaphore

                            myJob = null; 
                        }
                        else
                        {
                            List<string> route = floorPlan.getRoute(position, myJob.dockNumber);
                            getPathDock(route);
                            GoTo();

                            double weightCheck = 0;

                            int counter = 1;

                            foreach (KeyValuePair<string, double[]> item in myJob.listOfItems)
                            {
                               
                                Item itemName = CentralComputer.productList.Read(item.Key.ToUpper());

                                weightCheck += itemName.itemWeight * itemName.orderedStock;

                                if(weightCheck > loadingCapacity)
                                {
                                    itemsOverload.Add(item.Key);
                                    weightCheck = 0;
                                    loadPickup.Add(load);
                                    load = new List<string>();

                                    if (counter == myJob.listOfItems.Count)
                                    {
                                        load.Add(item.Key);
                                        loadPickup.Add(load);
                                        load = new List<string>();
                                    }
                                }
                                else
                                {
                                    load.Add(item.Key);

                                    if (counter == myJob.listOfItems.Count)
                                    {
                                        loadPickup.Add(load);
                                    }
                                }
                                counter++;
                            }

                            int count = 0;

                            foreach (KeyValuePair<string, double[]> item in myJob.listOfItems)
                            {

                                Item itemName = CentralComputer.productList.Read(item.Key.ToUpper());

                                if (itemsOverload.Contains(item.Key))
                                {
                                    route = floorPlan.getRoute(position, myJob.dockNumber);
                                    getPathDock(route);
                                    GoTo();
                                }

                                if (loadingCapacity == 500)
                                {
                                    Console.WriteLine("count: " + loadPickup[count].Count);

                                    foreach (string items in loadPickup[count])
                                    {
                                        pickUpItem(CentralComputer.productList.Read(items));
                                        Console.WriteLine("robot picked up the items at the dock");
                                    }
                                    count++;
                                }

                                route = floorPlan.getRoute(position, itemName.vertexLocation);

                                position = itemName.vertexLocation;
                                getPath(route);
                                GoTo();

                                //for the getting delivery version this needs to call execute order
                                executeStocking(itemName);
                            }
                            GoHome();
                            loadingCapacity = 500;
                            loadPickup.Clear();
                            load.Clear();
                            //need to realse dock semaphore

                            //issue?
                            if(myJob.dockNumber == "C5")
                            {
                                Dock1.dock1.Release();
                            }
                            else if (myJob.dockNumber == "D5")
                            {
                                Dock2.dock2.Release();
                            }

                            myJob = null; //added*/
                        }
                    }
                }

            }
        }

        public void updateFloorPlan(FloorPlan newFloorPlan)
        {
            floorPlan = newFloorPlan;
        }

        //combine the get path functions
        public void getPath(List<string> route)
        {
            for (int i = route.Count - 1; i >= 0; i--)
            {
                pathTo.Enqueue(route[i]);
            }

        }

        public void getPathDock(List<string> route)
        {
            for (int i = route.Count - 1; i >= 0; i--)
            {
                pathTo.Enqueue(route[i]);
            }
        }


        public void getPathHome(List<string> route)
        {
            for (int i = route.Count - 1; i >= 0; i--)
            {
                pathHome.Enqueue(route[i]);
            }

        }

        public void GoTo()
        {
            while (pathTo.Count != 0)
            {
                position = pathTo.Peek();

                Console.WriteLine("position: " + position);
                pathTo.Dequeue();
                Thread.Sleep(1000);
            }
        }

        public void GoHome()
        {

            List<string> route = floorPlan.getRoute(position, home);
            getPathHome(route);
            while (pathHome.Count != 0)
            {
                position = pathHome.Peek();
                Console.WriteLine("position: " + position);
                pathHome.Dequeue();
                Thread.Sleep(1000);
            }

            Console.WriteLine("job complete");
        }

        public void executeStocking(Item item)
        {
            //need to modify in the cache lock?
            item.inStockInventory += item.orderedStock;

            //decrement the load that the robot is carryying

            Console.WriteLine(item.orderedStock + " of " + item.itemName + " stocked");

            putDownItem(item);

            item.orderedStock = 0;

        }

        public void executeOrderRetrival(Item item, double amountOrdered)
        {
            //need to modify in the cache lock?
            item.inStockInventory -= amountOrdered; //decrement by the amount ordered

            //increment the load that the robot is carryying

            Console.WriteLine(myJob.listOfItems[item.itemName][0] + " of " + item.itemName + " retrieved");

            pickUpItem(item);

            //some kind of logic hear to trigger central computer to restock?
        }

        public void pickUpItem(Item itemName)
        {
            if (myJob.isOrder)
            {
                loadingCapacity -= itemName.itemWeight * myJob.listOfItems[itemName.itemName][0];
                Console.WriteLine("The robot is picking up: " + itemName.itemName);
            }
            else
            {
                loadingCapacity -= itemName.itemWeight * itemName.orderedStock;
                Console.WriteLine("The robot is picking up: " + itemName.itemName);
            }
             
                
        }

        public void putDownItem(Item itemName)
        {
            if (myJob.isOrder)
            {
                loadingCapacity += itemName.itemWeight * myJob.listOfItems[itemName.itemName][0];
                Console.WriteLine("Robot its carrying: " + loadingCapacity);
            }
            else
            {
                loadingCapacity += itemName.itemWeight * itemName.orderedStock;
                Console.WriteLine("Robot its carrying: " + loadingCapacity);
            }
        }
    }
}
