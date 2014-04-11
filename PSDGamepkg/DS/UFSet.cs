using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSD.PSDGamepkg.DS
{
    public class UFSet<T>
    {
        private List<int> father;
        private List<T> tplink;

        public UFSet()
        {
            tplink = new List<T>();
            father = new List<int>();
        }
        public T FindFather(T elem)
        {
            int re = FindFatherIndex(elem);
            return re < 0 ? elem : tplink[re];
        }
        private int FindFatherIndex(T elem)
        {
            int r = tplink.IndexOf(elem);
            return r < 0 ? r : FindFatherIndex(r);
        }
        private int FindFatherIndex(int r)
        {
            if (father[r] < 0)
                return r;
            else
                return father[r] = FindFatherIndex(father[r]);
        }

        public void Insert(T elem)
        {
            if (!tplink.Contains(elem))
            {
                tplink.Add(elem);
                father.Add(-1);
            }
        }

        public void Connect(T ti, T tj)
        {
            int ri = tplink.IndexOf(ti), rj = tplink.IndexOf(tj);
            if (father[ri] != -1 && father[rj] != -1)
            {
                int fj = FindFatherIndex(rj);
                father[fj] = ri;
            } else if (father[rj] == -1)
                father[rj] = ri;
            else
                father[ri] = rj;
        }
    }
}

//bool UFSet::UnionBySize(int posI, int posJ)//按大小求并
//{
////首先各自去寻找自己的族长
//int fI = FindFatherAndReducePath(posI);
//int fJ = FindFatherAndReducePath(posJ);
//if (fI == fJ) //如果是同一个族长门下，不必合并，即合并失败
//return false;
//else if (father[fI] < father[fJ])
//{//如果族长fI的实力比fJ强，即|fI|>|fJ|，则fI当族长，并修改father[fI]和father[fJ]
//father[fI] += father[fJ];
//father[fJ] = fI;
//}
//else //否则fJ当族长
//{
//father[fJ] += father[fI];
//father[fI] = fJ;
//}
//return true;
//}
//bool UFSet::SameFamily(int posI, int posJ)//判断成员posI和posJ是否属于同一家族
//{
//return FindFatherAndReducePath(posI) == FindFatherAndReducePath(posJ);
//}
//void UFSet::PrintUFSet()//输出集合的所有元素
//{
//for (int i=0; i<=size; i++)
//cout << father[i] << ' ';
//cout << endl;
//}
//}
