using System.Collections.Generic;
using System.Linq;

namespace IA.PathFinding.Utility
{
    public static class PathFindExtensions
    {
        ///<Sumary>
		/// Retorna el recorrido óptimo generado por uno de los algoritmos de búsqueda de caminos.
		///</Sumary>
        ///<param name="parents">Diccionario de hijos->Padres.</param>
		///<param name= "finalNode">Último nodo de la cadena.</param>
		public static IEnumerable<T> getCorrectPath<T>(this Dictionary<T,T> parents,T finalNode)
        {
            T currentNodeParent = parents[finalNode];

            if (currentNodeParent.Equals(parents[currentNodeParent]) || currentNodeParent == null)
                return Enumerable.Empty<T>()
                                 .Append<T>(currentNodeParent)
                                 .Append<T>(finalNode);

            return Enumerable.Empty<T>()
                             .Union<T>(getCorrectPath(parents, currentNodeParent))
                             .Append<T>(finalNode);
        }
    }
}
