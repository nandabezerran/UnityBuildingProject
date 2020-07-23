using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    enum TypesSurface {
        OUTWALL,
        INWALL,
        ROOF,
        TOPROOF,
        BASE
    }

    class Janela
    {
        public float distanciaHorizontal;
        public float distanciaEsquerda;
        public float distanciaDireita;
    }

    class Superficie
    {
        private int vertId;
        private List<int> _refPontos;
        private TypesSurface _type;
        private Janela _janela;

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

        public Superficie(List<int> refPontos, TypesSurface type, Casa casa)
        {
            _refPontos = refPontos;
            _type = type;
            _casa = casa;
        }

        public void AdicionarJanela(float centro, float largura, float altura)
        {
            Janela novaJanela = new Janela();
            novaJanela.distanciaHorizontal = (1f - altura) / 2f;
            novaJanela.distanciaEsquerda = centro - (largura / 2f);
            novaJanela.distanciaDireita = (1 - centro) + (largura / 2f);
            _janela = novaJanela;
        }

        public void AdicionaSuperficie(ref List<List<int>> triangulos, ref List<List<Vector2>> uvs)
        {
            int subMeshId = _casa.GetSubMeshParaSuperficie(_type);
            //Debug.Log(string.Join(", ", _refPontos));
            //Debug.Log(string.Join(", ", _refPontos.ConvertAll(_casa.GetPonto)));
            for (int i = 0; i < _casa.GetNumSubMeshs(); i++)
            {
                if(_janela != null)
                {
                    uvs[i].Add(new Vector2(0f, 0f));
                    uvs[i].Add(new Vector2(0f, _janela.distanciaHorizontal));
                    uvs[i].Add(new Vector2(0f, 1f - _janela.distanciaHorizontal));
                    uvs[i].Add(new Vector2(0f, 1f));
                    uvs[i].Add(new Vector2(_janela.distanciaDireita, _janela.distanciaHorizontal));
                    uvs[i].Add(new Vector2(_janela.distanciaDireita, 1f - _janela.distanciaHorizontal));
                    uvs[i].Add(new Vector2(_janela.distanciaEsquerda, 1f - _janela.distanciaHorizontal));
                    uvs[i].Add(new Vector2(_janela.distanciaEsquerda, _janela.distanciaHorizontal));
                    uvs[i].Add(new Vector2(1f, 1f));
                    uvs[i].Add(new Vector2(1f, 1f - _janela.distanciaHorizontal));
                    uvs[i].Add(new Vector2(1f, _janela.distanciaHorizontal));
                    uvs[i].Add(new Vector2(1f, 0f));
                }
                else
                {
                    uvs[i].AddRange(i == subMeshId ? _uvs : _blankUvs);
                }
            }

            if (_janela != null)
            {
                triangulos[subMeshId].AddRange(new List<int>{
                    vertId, vertId + 1, vertId + 10,
                    vertId, vertId + 10, vertId + 11,
                    vertId + 1, vertId + 2, vertId + 4,
                    vertId + 1, vertId + 4, vertId + 5,
                    vertId + 2, vertId + 3, vertId + 8,
                    vertId + 2, vertId + 8, vertId + 9,
                    vertId + 6, vertId + 7, vertId + 9,
                    vertId + 6, vertId + 9, vertId + 10,
                });
            }
            else
            {
                triangulos[subMeshId].AddRange(new List<int>{
                    vertId, vertId + 1, vertId + 2,
                    vertId, vertId + 2, vertId + 3
                });
            }
        }

        internal void UpdateVertices(ref List<Vector3> vertices)
        {
            vertId = vertices.Count;
            if (_janela != null)
            {
                Vector3[] windowPoints = new Vector3[12];

                windowPoints[0] = _casa.GetPonto(_refPontos[0]);
                windowPoints[3] = _casa.GetPonto(_refPontos[1]);
                windowPoints[8] = _casa.GetPonto(_refPontos[2]);
                windowPoints[11] = _casa.GetPonto(_refPontos[3]);

                Vector3 vectorAB = windowPoints[3] - windowPoints[0];
                Vector3 vectorDC = windowPoints[8] - windowPoints[11];

                windowPoints[1] = (vectorAB * _janela.distanciaHorizontal) + windowPoints[0];
                windowPoints[2] = (vectorAB * (1f - _janela.distanciaHorizontal)) + windowPoints[0];
                windowPoints[9] = (vectorDC * (1f - _janela.distanciaHorizontal)) + windowPoints[11];
                windowPoints[10] = (vectorDC * _janela.distanciaHorizontal) + windowPoints[11];

                Vector3 vectorW1W8 = windowPoints[10] - windowPoints[1];
                Vector3 vectorW2W7 = windowPoints[9] - windowPoints[2];

                windowPoints[4] = (vectorW2W7 * _janela.distanciaEsquerda) + windowPoints[2];
                windowPoints[5] = (vectorW1W8 * _janela.distanciaEsquerda) + windowPoints[1];
                windowPoints[6] = (vectorW1W8 * _janela.distanciaDireita) + windowPoints[1];
                windowPoints[7] = (vectorW2W7 * _janela.distanciaDireita) + windowPoints[2];

                vertices.AddRange(windowPoints);
            }
            else
            {
                vertices.AddRange(_refPontos.ConvertAll(_casa.GetPonto));
            }
        }
    }
}
