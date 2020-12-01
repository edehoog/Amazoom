using System;
using System.Collections.Generic;
using System.Text;

namespace Amazoom
{
    //need to keep an amount representing coming in and out? see if errors later

    public class Item
    {
        public string itemName;
        public double inStockInventory = 0; //potential error
        public double orderedStock = 0;
        public string vertexLocation;
        public string side;
        //public double price;
        public double itemWeight;
        public double itemVolume;
        bool inStock;
        //double array holds inventory amount and the wieght of the inventory (for the specified shelf at the above specified vertex)
        public Dictionary<int, double[]> shelfNumberInventory;
        public Item(string ItemName, string VertexLocation, string Side, Dictionary<int, double[]> ShelfNumberInventory, double ItemWeight, double ItemVolume, double OrderedStock)
        {
            itemName = ItemName;
            vertexLocation = VertexLocation;
            side = Side;
            shelfNumberInventory = ShelfNumberInventory;
            foreach (KeyValuePair<int, double[]> entry in ShelfNumberInventory)
            {
                inStockInventory += entry.Value[0];
            }
            //price = Price;
            itemWeight = ItemWeight;
            inStock = false;
            itemVolume = ItemVolume;
            orderedStock = OrderedStock;
        }
    }

    public class Job
    {
        public DateTime EnqueueDateTime { get; set; }
        public DateTime DequeueDateTime { get; set; }
        public DateTime ExecutionDateTime { get; set; }
        public string QueueID { get; set; }
        public Dictionary<string, double[]> listOfItems { get; set; }

        public bool isOrder;

        public string dockNumber;

    }

    public class Stock : Job
    {
    }

    public class Order : Job
    {
    }
}
