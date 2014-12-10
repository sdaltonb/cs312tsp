using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;


namespace TSP
{
    class PriorityQueue
    {
        private TSPSolution[] solutions;
        private uint lastIndex;


        public PriorityQueue(uint size) 
        {
            solutions = new TSPSolution[size];
            lastIndex = 0;
        }

        public void push(TSPSolution solution)
        {
            solutions[lastIndex] = solution;
            percolateUp(lastIndex);
            ++lastIndex;
        }

        public TSPSolution pop()
        {
            TSPSolution temp = solutions[0];
            solutions[0] = solutions[lastIndex - 1];

            percolateDown(0);
            --lastIndex;

            return temp;
        }

        public TSPSolution peek()
        {
            return solutions[0];
        }

        public uint getSize()
        {
            return lastIndex;
        }

        public void clear() 
        {
            lastIndex = 0;
        }

        private void percolateUp(uint index)
        {
            if (index == 0)
            {
                return;
            }

            if (solutions[(int)index].getLength() < solutions[(int)(index - 1) / 2].getLength())
            {
                TSPSolution temp = solutions[(uint)index];
                solutions[(uint)index] = solutions[(uint)(index - 1) / 2];
                solutions[(uint)(index - 1) / 2] = temp;

                percolateUp((index - 1)/ 2);
            }
        }

        private void percolateDown(uint siftingIndex)
        {
            uint minimumIndex;
            uint leftChildIndex = (siftingIndex * 2) + 1;
            uint rightChildIndex = (siftingIndex + 1) * 2;

            if (rightChildIndex < lastIndex)
            {
                minimumIndex = solutions[rightChildIndex].getLength() < solutions[leftChildIndex].getLength() ? rightChildIndex : leftChildIndex;
            }
            else if (leftChildIndex < lastIndex)
            {
                minimumIndex = leftChildIndex;
            }
            else
            {
                return;
            }

            if (solutions[siftingIndex].getLength() > solutions[minimumIndex].getLength())
            {
                TSPSolution temp = solutions[siftingIndex];
                solutions[siftingIndex] = solutions[minimumIndex];
                solutions[minimumIndex] = temp;

                percolateDown(minimumIndex);
            }
        }
    }
}
