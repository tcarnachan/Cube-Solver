using UnityEngine;
using System.Threading;
using Cube_Solver.Solver;

public class Test : MonoBehaviour
{
    private Search solver;

    void Start()
    {
        Resources.Load(Search.coPath);
        Resources.Load(Search.cpPath);
        Resources.Load(Search.eoPath);
        Resources.Load(Search.epPath);
        solver = new Search("UUUUUUUUULLLLLLLLLFFFFFFFFFRRRRRRRRRBBBBBBBBBDDDDDDDDD", "Assets/Resources/", Debug.Log);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
            new Thread(() => solver.Solve("lflburfldfdrllururuflbffdlrbburrdublbudrbdufdfubrdlbdf")).Start();
        if (Input.GetKeyDown(KeyCode.Return))
            solver.exit = true;
    }
}
