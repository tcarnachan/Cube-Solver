using System;

namespace Cube_Solver.Cubes
{
    class MoveData
    {
        private int[] cp, co, ep, eo;

        public MoveData(CubieCube cc)
        {
            cp = new int[cc.cp.Length];
            Array.Copy(cc.cp, cp, cp.Length);

            co = new int[cc.co.Length];
            Array.Copy(cc.co, co, co.Length);

            ep = new int[cc.ep.Length];
            Array.Copy(cc.ep, ep, ep.Length);

            eo = new int[cc.eo.Length];
            Array.Copy(cc.eo, eo, eo.Length);
        }

        public CubieCube ApplyMove(CubieCube cc)
        {
            int[] ncp = new int[cp.Length], nco = new int[co.Length], nep = new int[ep.Length], neo = new int[eo.Length];

            for (int i = 0; i < cp.Length; i++)
                ncp[i] = cc.cp[cp[i]];

            for (int i = 0; i < co.Length; i++)
                nco[i] = (cc.co[cp[i]] + co[i]) % 3;

            for (int i = 0; i < ep.Length; i++)
                nep[i] = cc.ep[ep[i]];

            for (int i = 0; i < eo.Length; i++)
                neo[i] = (cc.eo[ep[i]] + eo[i]) % 2;

            return new CubieCube(ncp, nco, nep, neo);
        }
    }
}
