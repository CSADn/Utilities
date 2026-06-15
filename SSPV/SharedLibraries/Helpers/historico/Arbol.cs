using System;
using System.Collections;
using System.Collections.Generic;
namespace Arboles
{
    public class Arbol : ICloneable
    {
        object valor;
        ArrayList hijos;

        //Constructor
        public Arbol(object valor)
        {
            this.valor = valor;
            this.hijos = new ArrayList();
        }

        //Constructor del arbol con hijos
        public Arbol(object valor, params Arbol[] hijos) : this(valor)
        {
            foreach (Arbol a in hijos)
                this.hijos.Add(a);
        }

        //Devuelve el valor asociado al arbol
        public object Valor
        {
            get
            {
                return this.valor;
            }
        }

        //Add un elemento (nodo) al arbol actual
        public void Add(Arbol x)
        {
            this.hijos.Add(x);
        }

        //Metodo para saber cuando un arbol es una hoja
        public bool EsHoja
        {
            get
            {
                return (this.hijos == null || this.hijos.Count == 0);
            }
        }

        //Ancho del arbol (cantidad de hojas)
        public int Ancho
        {
            get
            {
                int temp = EsHoja ? 1 : 0;
                foreach (Arbol a in this.hijos)
                    temp += a.Ancho;
                return temp;
            }
        }

        //Altura del arbol
        public int Alto
        {
            get
            {
                if (TotalHijos == 0)
                    return 0;
                else
                {
                    int max = 0;
                    foreach (Arbol a in hijos)
                    {
                        int altura = a.Alto;
                        if (altura > max)
                            max = altura;
                    }
                    return max + 1;
                }
            }
        }

        //Propiedad q devuelve la cantidad de hijos
        public int TotalHijos
        {
            get
            {
                if (hijos == null) return 0;
                else return hijos.Count;
            }
        }

        //Recorrido por niveles
        public List<object> ALoAncho()
        {
            List<object> li = new List<object>();
            li.Add(this);
            for (int i = 0; i < li.Count; i++)
            {
                Arbol a = (Arbol)li[i];
                foreach (Arbol hijo in a.hijos)
                    li.Add(hijo);
                li[i] = a.Valor;
            }
            return li;
        }

        //Devuelve en una lista: Primero los hijos a los padres..
        public void PostOrden(List<object> li)
        {
            if (this.EsHoja)
            {
                if(!li.Contains(this.valor))
                    li.Add(this.Valor);
            }

            else
            {
                foreach (Arbol hijo in this.hijos)
                    hijo.PostOrden(li);
                {
                    if (!li.Contains(this.valor))
                        li.Add(this.Valor);
                }
                
            }
        }

        //Devuelve una lista: Primero los padres
        public void PreOrden(List<object> li)
        {
            if (!li.Contains(this.valor))
                li.Add(this.Valor);
            foreach (Arbol hijo in this.hijos)
                hijo.PreOrden(li);

        }

        //Devuelve si el arbol contiene el elemento x
        public bool Contiene(object x)
        {
            if (this.valor.Equals(x)) return true;
            else
            {
                foreach (Arbol a in this.hijos)
                    if (a.Contiene(x)) return true;

                return false;
            }
        }

        //Devuelve el Arbol cuya raiz es el Arbol especificado
        public Arbol DameArbol(object x)
        {
            if (this.valor.Equals(x))
                return this;
            else
            {
                Arbol temp = null;
                foreach (Arbol a in this.hijos)
                {
                    temp = a.DameArbol(x);
                    if (temp != null)
                        return temp;
                }
                return null;
            }
        }

        //Retorna true si el valor valor1 esta en la raiz de un Arbol que contiene al valor valor2
        public bool EsAncestro(object valor1, object valor2)
        {
            if (this.Contiene(valor1))
                return this.DameArbol(valor1).Contiene(valor2);
            return false;
        }

        // Retorna true si los valores especificados tienen una raiz en comun
        public bool SeranHermanos(object valor1, object valor2)
        {
            bool hermanoA1 = false;
            bool hermanoA2 = false;
            foreach (Arbol hijo in this.hijos)
            {
                if (hijo.Valor.Equals(valor1))
                    hermanoA1 = true;
                if (hijo.Valor.Equals(valor2))
                    hermanoA2 = true;
            }
            if (hermanoA1 && hermanoA2)
                return true;
            else
            {
                foreach (Arbol hijo in this.hijos)
                {
                    if (hijo.SeranHermanos(valor1, valor2))
                        return true;
                }
            }
            return false;
        }

        #region ICloneable Members
        /// Crea una copia exacta del Arbol
        public object Clone()
        {
            Arbol clon;
            if (this.valor is ICloneable)
            {
                ICloneable clon_valor = (ICloneable)this.valor;
                clon = new Arbol(clon_valor.Clone());
            }
            else
                clon = new Arbol(this.valor);
            if (this.EsHoja)
                return clon;
            else
            {
                foreach (Arbol a in this.hijos)
                    clon.Add((Arbol)a.Clone());
                return clon;
            }
        }
        #endregion

        //Retorna una copia exacta del arbol
        public Arbol Espejo()
        {
            Arbol clon;
            if (this.valor is ICloneable)
            {
                ICloneable clon_valor = (ICloneable)this.valor;
                clon = new Arbol(clon_valor);
            }
            else
                clon = new Arbol(this.valor);
            if (this.EsHoja)
                return (Arbol)this.Clone();
            else
            {
                ArrayList nuevos_hijos = new ArrayList();
                for (int i = this.hijos.Count - 1; i >= 0; i--)
                {
                    Arbol hijo = (Arbol)this.hijos[i];
                    clon.Add(hijo.Espejo());
                }
                return clon;
            }
        }

        //Imprime el arbol
        public void PrintTree(string s)
        {
            //Console.WriteLine(s + this.valor);
            //s += " ==> ";
            s += " ==> " + this.valor;
            foreach (Arbol a in this.hijos)
            {
                a.PrintTree(s);
            }
        }

        //iterador para usar en el recorrido Preorden
        private class PreordenEnumerator : IEnumerator
        {
            Arbol a;
            int index;
            PreordenEnumerator e;
            object current;
            public PreordenEnumerator(Arbol a)
            {
                this.a = a;
                index = -1;
            }
            #region IEnumerator Members
            public void Reset()
            {
                index = -1;
                current = null;
                e = null;
            }

            public object Current
            {
                get
                {
                    if (index < 0 || index > a.hijos.Count)
                        throw new InvalidOperationException();
                    else return current;
                }
            }

            public bool MoveNext()
            {
                if (e == null)
                {
                    index++;
                }
                if (index == 0)
                {
                    current = a.valor;
                    return true;
                }
                if (index <= a.hijos.Count)
                {
                    if (e == null)
                        e = new PreordenEnumerator((Arbol)a.hijos[index - 1]);
                    if (e.MoveNext())
                    {
                        current = e.Current;
                        return true;
                    }
                    e = null;
                    return MoveNext();
                }
                return false;
            }

            #endregion
        }
    }
}
