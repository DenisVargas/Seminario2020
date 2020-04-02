using System.Collections;
using System.Collections.Generic;

public class DinamicArray<T> : IDinamicArray<T>, IEnumerable<T>
{
	T[] array;
	public int Count{get; private set;}

	public DinamicArray()
	{
		array = new T[1];
		Count = 0;
	}
	public DinamicArray(int initialElementCount)
	{
		array = new T[initialElementCount * 2];
		this.Count = 0;
	}

	public T this[int index]
	{
		get => array[index];
		set => array[index] = value;
	}

	public void Add(T element)
	{
		Count++;
		if (Count < array.Length)
			array[Count - 1] = element;
		else
		{
			T[] newArray = new T[Count * 2];
			for (int i = 0; i < array.Length; i++)
			{
				if (i > Count) break;
				if (array[i] == null) continue;

				if (i == Count - 1)
					newArray[Count - 1] = element;
				else
					newArray[i] = array[i];
			}
			array = newArray;
		}
	}

	/// <summary>
	/// Inserta un elemento nuevo en el índice dado desplazando los consecuentes.
	/// </summary>
	/// <param name="element">Elemento a insertar.</param>
	/// <param name="index">Índice del nuevo elemento.</param>
	public void Insert(T element, int index)
	{
        Count = index < Count ? (index + 1) : (Count + 1);
        T[] newArray = array.Length > Count ? new T[array.Length] : new T[Count * 2];

        for (int i = 0; i < newArray.Length; i++)
		{
            if(i == index) newArray[i] = element;
            else
            if (i < array.Length)
            {
                if (array[i] == null) continue;
                if (i < index) newArray[i] = array[i];
                if (i > index) newArray[i] = array[i - 1];
            }
            else
            {
                if (i < index) continue;
                if (i > index) break;
            }
		}
		array = newArray;
	}

	/// <summary>
	/// Limipia el array dinamico.
	/// </summary>
	public void Clear()
	{
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = default(T);
		}
		Count = 0;
	}

	/// <summary>
	/// Determina si el valor dado existe en el array.
	/// </summary>
	/// <returns>True si el valor existe.</returns>
	public bool Contains(T element)
	{
		for (int i = 0; i < Count; i++)
			if( array[i].Equals(element)) return true;
		return false;
	}

	/// <summary>
	/// Devuelve el índice del primer valor encontrado que coincida con el parámetro, si este existe en el Array.
    /// Retorna -1 si no se encontró ninguna coincidencia.
	/// </summary>
	/// <returns>Índice del valor dado.</returns>
	public int IndexOf(T element)
	{
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Equals(element)) return i;
		}
		return -1;
	}

	/// <summary>
	/// Remueve el primer valor que coincida con el parámetro dado.
	/// </summary>
	/// <param name="element">Valor a eliminar.</param>
	public void Remove(T element)
	{
		bool removed = false;
		for (int i = 0; i < Count; i++)
			if (array[i].Equals(element))
			{
				removed = true;
				T[] newArray = new T[array.Length];
				for (int J = 0; J < Count -1; J++) newArray[J] = J < i ? array[J] : array[J + 1];
				array = newArray;
				break;
			}
		if (removed) Count--;
	}

	/// <summary>
	/// Remueve el elemento en el índice dado.
	/// </summary>
	/// <param name="index">Indice del elemento a eliminar.</param>
	public void RemoveAt(int index)
	{
        if (index >= 0 && index < Count)
        {
            Count--;
            T[] newArray = new T[array.Length];
            for (int i = 0; i < Count; i++)
                newArray[i] = i < index ? array[i] : array[i + 1];
            array = newArray;
        }
        else
            throw new System.IndexOutOfRangeException(
                index < 0 ? "El Índice dado es Inválido\nNo se aceptan valores negativos" : "El Índice dado es Inválido\nEl Índice está por fuera de los límites del Array");
	}

	public IEnumerator<T> GetEnumerator()
	{
		for (int i = 0; i < Count; i++)
			yield return array[i];
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		yield return GetEnumerator();
	}
}
//Pregunta: Como devuelvo NULL si estoy usando genéricos y no se que tipo es el "Default";
//Respuesta: Default devuelve 0 cuando el valor por defecto es un int-float-double(numèricos)
//Si se trata de Clases, devolverá NULL como Default.
