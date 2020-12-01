using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Amazoom
{
    class CentralComputer
    {
        //key is product name, value is string array of location identifiers
        //public static Dictionary<string, Item> productList; //this was static

        public static SynchronizedCache productList;

        public static bool isEmpty;

       // public static MySynchronizedQueue<Job> taskQueue;

        int numberOfRobots;

        FloorPlan floorPlan;

        //Task[] robotThreads;

        Thread[] truckThreads;

        Robot[] robots;

        Truck[] trucks;

        string[] leftOrRight = { "L", "R" };

        Random random = new Random();

        int numberOfShelves = 6;

        int numberOfTrucks;

        double shelfCapacity = 100.00; //(kg)

        double truckVolumeCapacity = 5.0; //cubic meters

        double truckWeightCapacity = 1000.0; //kg

        public CentralComputer(int NumberOfRobots, int NumberOfTrucks)
        {
            numberOfRobots = NumberOfRobots;
            //taskQueue = new MySynchronizedQueue<Job>();
            productList = new SynchronizedCache();
            floorPlan = new FloorPlan();
            loadFloorPlan();
            robots = new Robot[NumberOfRobots];
            //robotThreads = new Task[NumberOfRobots];

            trucks = new Truck[NumberOfTrucks];
            numberOfTrucks = NumberOfTrucks;
            isEmpty = true;
        }

        public void StartRobots(int numberOfRobots)
        {
            //robotThreads = new Task[numberOfRobots];
            for (int i = 0; i < numberOfRobots; i++)
            {
                string robotName = "Robot" + (i + 1).ToString();
                Console.WriteLine(robotName + " was created");
                robots[i] = new Robot(robotName, floorPlan);
                robots[i].turnOnRobot(); // added
                //Robot robot = robots[i];
                //robotThreads[i] = new Task(() => robot.startRobot());
                //robotThreads[i].Start();
                //Console.WriteLine("thread started.");
            }
        }

        public void StartTrucks(int numberOfTrucks)
        {
            truckThreads = new Thread[numberOfTrucks];
            for (int i = 0; i < numberOfTrucks; i++)
            {
                string truckName = "Truck" + (i + 1).ToString();
                Console.WriteLine(truckName + " was created");
                trucks[i] = new Truck(truckName, truckVolumeCapacity, truckWeightCapacity); // need to get the number of delivery trucks and number of stocking
                Truck truck = trucks[i];
                truckThreads[i] = new Thread(() => truck.startTruck());
                truckThreads[i].Start();
                Console.WriteLine("thread started.");
            }
        }

        /*
        private void StopRobots()
        {
            //need to send some stop signal (make it a bool that stops the infinite while loop of the robots)
            for (int i = 0; i < numberOfRobots; i++)
            {
                robotThreads[i].Wait();
            }
        }*/

        private void loadFloorPlan()
        {
            floorPlan.addVertex("A1", false, false, 0);
            floorPlan.addVertex("A2", false, true, numberOfShelves);
            floorPlan.addVertex("A3", false, true, numberOfShelves);
            floorPlan.addVertex("A4", false, true, numberOfShelves);
            floorPlan.addVertex("A5", false, false, 0);
            floorPlan.addVertex("A6", false, false, 0);
            floorPlan.addVertex("B1", false, false, 0);
            floorPlan.addVertex("B2", true, true, numberOfShelves);
            floorPlan.addVertex("B3", true, true, numberOfShelves);
            floorPlan.addVertex("B4", true, true, numberOfShelves);
            floorPlan.addVertex("B5", false, false, 0);
            floorPlan.addVertex("C1", false, false, 0);
            floorPlan.addVertex("C2", true, true, numberOfShelves);
            floorPlan.addVertex("C3", true, true, numberOfShelves);
            floorPlan.addVertex("C4", true, true, numberOfShelves);
            floorPlan.addVertex("C5", false, false, 0);
            floorPlan.addVertex("D1", false, false, 0);
            floorPlan.addVertex("D2", true, true, numberOfShelves);
            floorPlan.addVertex("D3", true, true, numberOfShelves);
            floorPlan.addVertex("D4", true, true, numberOfShelves);
            floorPlan.addVertex("D5", false, false, 0);

            floorPlan.addEdge("A1", "A2");
            floorPlan.addEdge("A2", "A3");
            floorPlan.addEdge("A3", "A4");
            floorPlan.addEdge("A4", "A5");
            floorPlan.addEdge("A5", "A6");
            floorPlan.addEdge("A5", "B5");
            floorPlan.addEdge("A1", "B1");
            floorPlan.addEdge("B1", "B2");
            floorPlan.addEdge("B2", "B3");
            floorPlan.addEdge("B3", "B4");
            floorPlan.addEdge("B4", "B5");
            floorPlan.addEdge("B1", "C1");
            floorPlan.addEdge("B2", "B3");
            floorPlan.addEdge("C1", "C2");
            floorPlan.addEdge("C2", "C3");
            floorPlan.addEdge("C3", "C4");
            floorPlan.addEdge("C4", "C5");
            floorPlan.addEdge("C5", "B5");
            floorPlan.addEdge("C1", "D1");
            floorPlan.addEdge("D1", "D2");
            floorPlan.addEdge("D2", "D3");
            floorPlan.addEdge("D3", "D4");
            floorPlan.addEdge("D4", "D5");
            floorPlan.addEdge("C5", "D5");
        }

        //need to add overstocking becasue of volume as well?
        //prevent any stock addition over robot carrying capacity
        public void addProduct(string itemName, double inventory, double weight, double volume)
        {
            double totalWeight = Convert.ToDouble(inventory) * weight;

            //TODO need to add cases for this and overstocking 
            //add shelf volume variable
            double totalVolume = Convert.ToDouble(inventory) * volume;

            List<Vertex> listOfVertex = floorPlan.getAdjList();

            Dictionary<int, double[]> shelfNumberInventory = new Dictionary<int, double[]>();

            double[] shelfInformation;

            int position;

            string vertexLocation;

            bool isShelfLeft;

            bool isShelfRight;

            string side = "not assigned";

            int shelfNumber; 

            string shelfName;

            bool underCapacity = false;

            if(totalWeight <= shelfCapacity)
            {
                Console.WriteLine("Here 234");
                do
                {
                    position = random.Next(floorPlan.numberOfVertex());
                    vertexLocation = listOfVertex[position].getName();
                    isShelfLeft = listOfVertex[position].shelfLeft;
                    isShelfRight = listOfVertex[position].shelfRight;
                    shelfNumber = random.Next(listOfVertex[position].numberShelves + 1);

                    if (isShelfLeft && isShelfRight)
                    {
                        side = leftOrRight[random.Next(2)];
                    }
                    else if (isShelfLeft)
                    {
                        side = "L";
                    }
                    else if (isShelfRight)
                    {
                        side = "R";
                    }
                    else
                    {
                        side = "not assigned";
                    }
                    if (side != "not assigned" && shelfNumber != 0)
                    {
                        shelfName = side + (shelfNumber).ToString();
                        Console.WriteLine("here: " + vertexLocation + ", " + shelfName);
                        if (listOfVertex[position].shelfLoad[shelfName] + totalWeight <= shelfCapacity) 
                        {
                            underCapacity = true;
                            Console.WriteLine("here qwrqwerqwerqwer: " + vertexLocation + ", " + shelfName);
                        }
                        else
                        {
                            underCapacity = false;
                            Console.WriteLine("hereasdiprewtiowejrtr: " + vertexLocation + ", " + shelfName);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Here");
                        continue;
                    }
                } while ((!isShelfLeft || !isShelfRight) && !underCapacity);

                shelfInformation = new double[2] { inventory, totalWeight };

                shelfNumberInventory[shelfNumber] = shelfInformation;
            }
            //TODO: Need to implement a robust method to add overstock to a different shelf number
            else
            {
                int iterations = Convert.ToInt32(Math.Ceiling(totalWeight / shelfCapacity));

                do
                {
                    position = random.Next(floorPlan.numberOfVertex());
                    vertexLocation = listOfVertex[position].getName();
                    isShelfLeft = listOfVertex[position].shelfLeft;
                    isShelfRight = listOfVertex[position].shelfRight;
                    shelfNumber = random.Next(listOfVertex[position].numberShelves + 1);

                    if (isShelfLeft && isShelfRight)
                    {
                        side = leftOrRight[random.Next(2)];
                    }
                    else if (isShelfLeft)
                    {
                        side = "L";
                    }
                    else if (isShelfRight)
                    {
                        side = "R";
                    }
                    else
                    {
                        side = "not assigned";
                    }
                } while ((!isShelfLeft || !isShelfRight) && shelfNumber == 0);

                double remainingInventory = inventory;

                Console.WriteLine("here: " + vertexLocation + ", " + side);

                //iterations can be greater than the number of shelves need to loop while under and then move to the next shelf
                for (int i = 1; i < numberOfShelves + 1; i++)
                {
                    shelfName = side + (i).ToString();
                    int itemsOnShelf = (int)Math.Floor((shelfCapacity - listOfVertex[position].shelfLoad[shelfName]) / weight);

                    //could be case where 1 item is left ( rounding down from floor?

                    if ((int)remainingInventory > itemsOnShelf)
                    {
                        shelfInformation = new double[2] { itemsOnShelf, Convert.ToDouble(itemsOnShelf) * weight };

                        shelfNumberInventory[i] = shelfInformation;
                        remainingInventory -= Convert.ToDouble(itemsOnShelf);
                    }
                    else
                    {
                        shelfInformation = new double[2] { inventory, Convert.ToDouble(itemsOnShelf) * weight };

                        shelfNumberInventory[i] = shelfInformation;
                        remainingInventory -= remainingInventory;
                    }

                }
            }

            //TODO: THis all needs to be changed, the item is not in stock until it is delivered so determine the
            //randomized positions but do not determine shelves etc that needs to be done after the 
            //shipment has arrived

            Item newItem = new Item(itemName, vertexLocation, side, shelfNumberInventory, weight, volume, inventory);
            productList.Add(itemName, newItem);
            Console.WriteLine(productList.Read(itemName).itemName);
            lock (InventoryTruckLock.SyncRoot)
            {
                Console.WriteLine("HERERWERWERW");
                MySynchronizedStockingQueue<string>.Enqueue(itemName);
                Console.WriteLine(";dfjkga;kljga;skjgasdf");
            }
        }

        public void ProduceOrder(Dictionary<string, double[]> itemsAndDetails)
        {
            Console.WriteLine("Producing Order Task");

            bool validOrder = IsValidOrder(itemsAndDetails);
            if (validOrder)
            {
                var order = new Order
                {
                    QueueID = "taskQueue",
                    EnqueueDateTime = DateTime.Now,
                    listOfItems = itemsAndDetails,
                    isOrder = true
                };

                lock(RobotTaskLock.SyncRoot)
                    MySynchronizedJobQueue<Job>.Enqueue(order);
                Console.WriteLine("Order Suceeded");
            }
            else
            {
                Console.WriteLine("Order Failed");
                return;
            }
        }

        private bool IsValidOrder(Dictionary<string, double[]> itemsAndDetails)
        {
            bool isValid = true;

            foreach (KeyValuePair<string, double[]> details in itemsAndDetails)
            {
                Item item;
                {
                    try
                    {
                        item = productList.Read(details.Key.ToUpper());
                        Console.WriteLine("Fetched value: {0}", item.itemName);
                        if (item.inStockInventory - details.Value[0] < 0)
                        {
                            isValid = false;
                        }
                    }
                    catch(KeyNotFoundException e)
                    {
                        Console.WriteLine("The following item does not exist: {0}", details.Key);
                        isValid = false;
                    }
                }
            }
            return isValid;
        }
    }

    static class Dock1
    {
        public static SemaphoreSlim dock1 = new SemaphoreSlim(1);

        public static bool stocking { get; set; }

    }

    static class Dock2
    {
        public static SemaphoreSlim dock2 = new SemaphoreSlim(1);

        public static bool stocking { get; set; }
    }


    static class RobotTaskLock
    {
        public static object baton = new object();
        public static object SyncRoot
        {
            get { return baton; }
        }
    }

    static class MySynchronizedJobQueue<Job>
    {
        static Queue<Job> theQueue = new Queue<Job>();
        public static void Enqueue(Job item)
        {
            lock (RobotTaskLock.SyncRoot)
            {
                theQueue.Enqueue(item);
            }

        }
        public static Job Dequeue()
        {
            lock (RobotTaskLock.SyncRoot)
            {
                return theQueue.Dequeue();
            }
        }
        public static int Count
        {
            get { lock (RobotTaskLock.SyncRoot) return theQueue.Count; }
        }
    }

    static class InventoryTruckLock
    {
        public static object baton = new object();
        public static object SyncRoot
        {
            get { return baton; }
        }
    }

    static class MySynchronizedStockingQueue<T>
    {
        static Queue<string> theQueue = new Queue<string>();

        static double shipmentWeight = 0;

        static double shipmentVolume = 0;
        public static void Enqueue(string item)
        {
            lock (InventoryTruckLock.SyncRoot)
            {
                Console.WriteLine("addidng to sync queue");
                theQueue.Enqueue(item);
                shipmentWeight += CentralComputer.productList.Read(item).itemWeight 
                    * CentralComputer.productList.Read(item).orderedStock;
                shipmentVolume += CentralComputer.productList.Read(item).itemVolume 
                    * CentralComputer.productList.Read(item).orderedStock;

                Console.WriteLine(shipmentWeight);
            }

        }
        public static string Dequeue()
        {
            lock (InventoryTruckLock.SyncRoot)
            {
                Console.WriteLine("There is a dequeue");
                var item = theQueue.Dequeue();
                //need to multiply by inventory?
                shipmentWeight -= CentralComputer.productList.Read(item).itemWeight 
                    * CentralComputer.productList.Read(item).orderedStock;
                shipmentVolume -= CentralComputer.productList.Read(item).itemVolume 
                    * CentralComputer.productList.Read(item).orderedStock;

                return item;
            }
        }
        public static int Count
        {
            get { lock (InventoryTruckLock.SyncRoot) return theQueue.Count; }
        }
        public static double getShipmentWeight()
        {
            lock (InventoryTruckLock.SyncRoot)
            {  
                return shipmentWeight; 
            }
        }
        public static double getShipmentVolume()
        {
            lock (InventoryTruckLock.SyncRoot)
            {
                return shipmentVolume;
            }
        }
    }
}
