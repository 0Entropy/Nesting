using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace Nesting.GeneticAlgorithm
{
    using UnityEngine;
    using DNAPair = GA.DNAPair;
    using DNA = GA.DNA;

    using Polygon = List<UnityEngine.Vector2>;

    class FPLibrary
    {
        Dictionary<DNAPair, Polygon> LibNFP;

        Dictionary<DNA, Polygon> LibIFP;
        
        Rect binBoundary;

        public FPLibrary ( Rect binBoundary)
        {
            this.binBoundary = binBoundary;
            LibNFP = new Dictionary<DNAPair, List<Vector2>> ( );
            LibIFP = new Dictionary<DNA, List<Vector2>> ( );
        }

        public Polygon Lookup(DNAPair pair )
        {

            if ( !LibNFP.Keys.Contains ( pair ) )
            {
                LibNFP.Add ( pair, pair.NFP );
            }
            return LibNFP [ pair ];
        }

        public Polygon Lookup(DNA dna)
        {
            if ( !LibIFP.Keys.Contains ( dna ) )
            {
                LibIFP.Add ( dna, dna.IFP(ref binBoundary ) );
            }
            return LibIFP [ dna ];
        }
    }
}
