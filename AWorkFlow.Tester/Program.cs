using AWorkFlow.Core.Builder;
using AWorkFlow.Core.Runner;
using System;

namespace AWorkFlow.Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello workflow!");
            // set workflow
            var workflow = WorkFlowBuilder.Create("NewOrder", "testorder")
                .First()
                .Success()
                .Build();
            // test data
            var orderData = new { };

            // start a new work with data
            var work = WorkFlowEngine.Worker.StartWork("NewOrder", orderData);

            Console.WriteLine("press any key to exit...");
            Console.ReadKey();
        }
    }
}
