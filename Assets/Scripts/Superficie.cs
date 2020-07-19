using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    enum TiposSuperficie {
        PAREDE,
        TETO,
        PISO
    }

    class Superficie
    {
        private int vertId;
        private List<int> _refPontos;
        private TiposSuperficie _tipo;
        private List<Vector2> _uvs = new List<Vector2> {
            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(1, 0),
        };
        private List<Vector2> _blankUvs = new List<Vector2> {
            new Vector2(0, 0),
            new Vector2(0, 0),
            new Vector2(0, 0),
            new Vector2(0, 0),
        };
        private Casa _casa;

        public Superficie(List<int> refPontos, TiposSuperficie tipo, Casa casa)
        {
            _refPontos = refPontos;
            _tipo = tipo;
            _casa = casa;
        }

        public void AdicionaSuperficie(ref List<List<int>> triangulos, ref List<List<Vector2>> uvs)
        {
            int subMeshId = _casa.GetSubMeshParaSuperficie(_tipo);
            //Debug.Log(string.Join(", ", _refPontos));
            //Debug.Log(string.Join(", ", _refPontos.ConvertAll(_casa.GetPonto)));
            for (int i = 0; i < _casa.GetNumSubMeshs(); i++)
            {
                uvs[i].AddRange(i == subMeshId ? _uvs : _blankUvs);
            }
            triangulos[subMeshId].AddRange(new List<int>{
                vertId, vertId + 1, vertId + 2,
                vertId, vertId + 2, vertId + 3
            });
        }

        internal void UpdateVertices(ref List<Vector3> vertices)
        {
            vertId = vertices.Count;
            vertices.AddRange(_refPontos.ConvertAll(_casa.GetPonto));
        }
    }
}
