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

        public void CarregaSuperficie(string[] triangulo1, string[] triangulo2)
        {
            List<int> pontos = new List<int>();
            pontos.Add(int.Parse(triangulo1[2]));
            pontos.Add(int.Parse(triangulo1[3]));
            pontos.Add(int.Parse(triangulo1[4]));
            pontos.Add(int.Parse(triangulo2[4]));

            TiposSuperficie tipo = TiposSuperficie.PAREDE;
            int tipoInt;

            if(int.TryParse(triangulo1[5], out tipoInt))
            {
                tipo = (TiposSuperficie)tipoInt;
            }
            else
            {
                switch(triangulo1[5].ToLower())
                {
                    case "wall":
                        tipo = TiposSuperficie.PAREDE;
                        break;
                    case "base":
                        tipo = TiposSuperficie.PISO;
                        break;
                    case "roof":
                        tipo = TiposSuperficie.TETO;
                        break;
                }
            }

            _superficies.Add(new Superficie(pontos, tipo, _casa));
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
