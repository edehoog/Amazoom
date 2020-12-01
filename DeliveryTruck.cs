using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Amazoom
{
    class Truck
    {
        private double remainingVolumeCapacity;
        private double remainingWeightCapacity;
        int linePostion { get; set; }
        private double totalVolumeCapacity;
        private double totalWeightCapacity;
        bool isDocked { get; set; }
        bool isFull { get; set; }
        bool loadingTimeIsUp { get; set; }

        string dock1 = "C5";
        string dock2 = "D5";

        string truckName;

        Random rand = new Random();

        public Truck(string TruckName, double TotalVolumeCapacity, double TotalWeightCapacity)
        {
            totalVolumeCapacity = TotalVolumeCapacity;
            remainingVolumeCapacity = TotalVolumeCapacity;
            totalWeightCapacity = TotalWeightCapacity;
            remainingWeightCapacity = TotalWeightCapacity;
            truckName = TruckName;
            //isStocking = IsStocking;
        }

        public void startTruck()
        {
            while (true)
            {
                //implement some logic that only allows to get a new task if not completing a new one
                Thread.Sleep(rand.Next(1000));
                lock (InventoryTruckLock.SyncRoot)
                {
                    //Console.WriteLine(name + " is waiting for its task.");
                    if ((MySynchronizedStockingQueue<string>.getShipmentWeight() > (remainingWeightCapacity / 2.0)) ||
                        (MySynchronizedStockingQueue<string>.getShipmentVolume() > (remainingVolumeCapacity / 2.0)))
                    {
                        Console.WriteLine(truckName + " is getting stock.");
                        Dictionary<string, double[]> itemsAndDetails = new Dictionary<string, double[]>();

                        string item;

                        while ((remainingWeightCapacity > 0 && remainingVolumeCapacity > 0) && MySynchronizedStockingQueue<string>.Count != 0)
                        {
                            item = MySynchronizedStockingQueue<string>.Dequeue();

                            Console.WriteLine(CentralComputer.productList.Read(item).itemName);

                            itemsAndDetails[CentralComputer.productList.Read(item).itemName] = new double[]
                            {
                                CentralComputer.productList.Read(item).orderedStock,
                                CentralComputer.productList.Read(item).orderedStock * CentralComputer.productList.Read(item).itemWeight
                            };

                            remainingWeightCapacity -= CentralComputer.productList.Read(item).itemWeight;
                            remainingVolumeCapacity -= CentralComputer.productList.Read(item).itemVolume;
                        }

                        Thread.Sleep(rand.Next(10000));

                        int dock = rand.Next(2);

                        if (dock == 1)
                        {
                            Console.WriteLine(truckName + " is Waiting in Line for dock 1");
                            Dock1.dock1.Wait();
                            isDocked = true;
                            Console.WriteLine(truckName + " is Docked.");
                            Dock1.stocking = true;
                            ProduceStockingTask(itemsAndDetails, dock1);
                            //some kind of unloading function while robots remove all items for stocking (some loop until all stock is removed truck just idles)
                            //use event wait handles to break the stocking loop?
                            CentralComputer.isEmpty = false; //this may need a lock
                        }
                        else
                        {
                            Console.WriteLine(truckName + " is Waiting in Line for dock 2");
                            Dock2.dock2.Wait();
                            isDocked = true;
                            Console.WriteLine(truckName + " is Docked.");
                            Dock2.stocking = true;
                            ProduceStockingTask(itemsAndDetails, dock2);
                            //some kind of unloading function while robots remove all items for stocking (some loop until all stock is removed truck just idles)
                            CentralComputer.isEmpty = false; //this may need a lock
                        }
                    }
                }
                if (CentralComputer.productList.Count != 0 && CentralComputer.isEmpty != true)
                {
                    int dock = rand.Next(2);

                    if (dock == 1)
                    {
                        Console.WriteLine(truckName + " is Waiting in Line for dock 1");
                        Dock1.dock1.Wait();
                        isDocked = true;
                        Console.WriteLine(truckName + " is Docked.");
                        Dock1.stocking = false;
                        //some kind of waiting function while robots move orders into the truck until it is full
                    }
                    else
                    {
                        Console.WriteLine(truckName + " is Waiting in Line for dock 2");
                        Dock2.dock2.Wait();
                        isDocked = true;
                        Console.WriteLine(truckName + " is Docked.");
                        Dock2.stocking = false;
                        //some kind of waiting function while robots move orders into the truck until it is full
                    }
                }
            }
                
        }

        private void ProduceStockingTask(Dictionary<string, double[]> itemsAndDetails, string dockVertex)
        {
            Console.WriteLine("Producing Stock Task");

            var order = new Stock
            {
                QueueID = "taskQueue",
                EnqueueDateTime = DateTime.Now,
                listOfItems = itemsAndDetails,
                isOrder = false,
                dockNumber = dockVertex
            };

            lock (RobotTaskLock.SyncRoot)
                MySynchronizedJobQueue<Job>.Enqueue(order);
            Console.WriteLine("Stocking job passed");
        }

        public double getRemainingWeightCapacity()
        {
            return remainingWeightCapacity;
        }
        public double getRemainingVolumeCapacity()
        {
            return remainingVolumeCapacity;
        }

        public void updatRemainingWeight(double weight)
        {
            remainingWeightCapacity -= weight;
        }

        public void updatRemainingVolume(double volume)
        {
            remainingVolumeCapacity -= volume;
        }
    }
}
