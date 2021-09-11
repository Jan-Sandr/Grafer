using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grafer2
{
    public class Function
    {
        Relation Relation { get; set; }
        double MinimumX {  get; set; }
        double MaximumX {  get; set; }
        CalculationOrder CalculationOrder {  get; set; }
        List<List<PointF>> Curves { get; set; }

        Relation relationBackup;
        CalculationOrder calculationOrderBackup;

        public Function(string relation,double minimumX, double maximumX)
        {
            Relation = new();
            Relation.AddRange(relation.Select(s => s.ToString()));
            MinimumX = minimumX;
            MaximumX = maximumX;
            CalculationOrder = new CalculationOrder() { new List<int>(), new List<int>() };
            Curves = new List<List<PointF>>();
        }

        public void PrepareForCalculation()
        {
            CalculationOrder = CalculationOrder.GetOrder(Relation, CalculationOrder);
        }

        public List<List<PointF>> CalculatePoints()
        {
            SetBackup();
            for(double x = MinimumX; x <= MaximumX; x += 0.01)
            {
                GetBackup();
                SubstituteX(x);
                //CalculateYForX();
            }
          
            return Curves;
        }

        private void SubstituteX(double x)
        {
            for(int i = 0;i< Relation.Count;i++)
            {
                Relation[i] = Relation[i] == "x" ? x.ToString() : Relation[i];
                Relation[i] = Relation[i] == "-x" ? (-x).ToString() : Relation[i];
            }
        }

        private void SetBackup()
        {
            relationBackup = new();
            relationBackup.AddRange(Relation);
            calculationOrderBackup = new();
            calculationOrderBackup.AddRange(CalculationOrder);
        }

        private void GetBackup()
        {
            Relation = new();
            CalculationOrder = new();
            Relation.AddRange(relationBackup);
            CalculationOrder.AddRange(calculationOrderBackup);
        }
    }
}
