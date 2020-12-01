using System;
using System.Collections.Generic;

namespace Amazoom
{
    class Program
    {
        // need some kind of pop up to display the stock of items and when to order
        static void Main(string[] args)
        {
            CentralComputer comp = new CentralComputer(2, 1);

            comp.StartRobots(2);

            comp.StartTrucks(1);

            while (true)
            {
                Console.WriteLine("Add a new product?");
                string answer = Console.ReadLine().ToString().ToLower();
                if(answer == "yes")
                {
                    Console.WriteLine("Enter item name: ");
                    string name = Console.ReadLine().ToUpper();

                    Console.WriteLine("Enter inventroy amount: ");

                    int inventory;

                    while (!int.TryParse(Console.ReadLine(), out inventory))
                    {
                        continue;
                    }

                    Console.WriteLine("Enter inventroy weight (per 1 item): ");

                    double weight;

                    while (!double.TryParse(Console.ReadLine(), out weight))
                    {
                        continue;
                    }

                    comp.addProduct(name, inventory, weight, 0.0000005);

                    Console.WriteLine("Get order?");
                    string ans = Console.ReadLine().ToLower();

                    if (ans == "yes")
                    {
                        Dictionary<string, double[]> orderList = new Dictionary<string, double[]>();

                        string booleanAns = "no";

                        do
                        {
                            Console.WriteLine("Enter item name: ");
                            string name2 = Console.ReadLine().ToUpper();
                            Console.WriteLine("Enter Quantity: ");
                            int quantity;

                            while (!int.TryParse(Console.ReadLine(), out quantity))
                                continue;

                            orderList[name2] = new double[] { Convert.ToDouble(quantity), CentralComputer.productList.Read(name).itemWeight };

                            Console.WriteLine("Add another item?");

                            booleanAns = Console.ReadLine().ToLower();

                        } while (booleanAns == "yes");



                        comp.ProduceOrder(orderList);
                    }

                }
            }
        }
    }
}
