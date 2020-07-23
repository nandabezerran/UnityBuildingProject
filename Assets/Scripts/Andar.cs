using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    class Andar
    {
        private int _id;

        private List<Superficie> _superficies = new List<Superficie>();

        private Casa _casa;

        public Andar(int id, Casa casa)
        {
            _id = id;
            _casa = casa;
        }

        public void CarregaSuperficie(string[] dadosSuperficie)
        {
            List<int> pontos = new List<int>();
            pontos.Add(int.Parse(dadosSuperficie[2]));
            pontos.Add(int.Parse(dadosSuperficie[3]));
            pontos.Add(int.Parse(dadosSuperficie[4]));
            pontos.Add(int.Parse(dadosSuperficie[5]));

            TypesSurface tipo = TypesSurface.OUTWALL;
            int tipoInt;

            if(int.TryParse(dadosSuperficie[6], out tipoInt))
            {
                tipo = (TypesSurface)tipoInt;
            }
            else
            {
                switch(dadosSuperficie[5].ToLower())
                {
                    case "outwall":
                        tipo = TypesSurface.OUTWALL;
                        break;
                    case "inwall":
                        tipo = TypesSurface.INWALL;
                        break;
                    case "base":
                        tipo = TypesSurface.BASE;
                        break;
                    case "roof":
                        tipo = TypesSurface.ROOF;
                        break;
                    case "toproof":
                        tipo = TypesSurface.TOPROOF;
                        break;
                }
            }

            Superficie newSuperficie = new Superficie(pontos, tipo, _casa);

            _superficies.Add(newSuperficie);
            
            if(dadosSuperficie.Length > 7)
            {
                newSuperficie.AdicionarJanela(
                    float.Parse(dadosSuperficie[8]),
                    float.Parse(dadosSuperficie[9]),
                    float.Parse(dadosSuperficie[10])
                );
            }
        }

        public void AdicionarAndar(ref List<List<int>> triangulos, ref List<List<Vector2>> uvs)
        {
            foreach(var sup in _superficies)
            {
                sup.AdicionaSuperficie(ref triangulos, ref uvs);
            }
        }

        internal void UpdateVertices(ref List<Vector3> vertices)
        {
            foreach(var sup in _superficies)
            {
                sup.UpdateVertices(ref vertices);
            }
        }
    }
}
