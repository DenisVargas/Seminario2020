using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;

public class DinamicArrayTests
{
    //Rules of TDD:
    //No esta permitido que:
    //1. Escribir código de producción a menos que sea para producir una prueba de unidad fallida.
    //2. Escribir más código que del que sea suficiente para fallar. (Errores de compilacion incluidos.)
    //3. Escribir más código de producción del que se necesita para pasar la prueba fallida.

    //Métodología (Red-Green Refactor Cicle):
    //1. Crea una UnitTest que falle.
    //2. Escribe código de producción que pase la prueba.
    //3. Refactoriza tu lío.

    [Test]
    [TestCase(new int[]{}, 0)]
    [TestCase(new int[]{2,3,4}, 3)]
    [TestCase(new int[]{2}, 2)]
    public void Test_DinamicArray_Add(
        int[] toAddElements,
        int expectedAmmountsOfElements )
    {
        //Arrange
        //creamos las instancias y todos los elementos necesarios para ejecutar el método.
        var array = new DinamicArray<int>(3);

        //Act
        //Llamamos el metodo pasándole los parámetros.
        for (int i = 0; i < toAddElements.Length; i++)
        {
            array.Add(toAddElements[i]);
        }

        //Assert
        //Usamos un assert para determinar si la prueba fue un exito o no.
        Assert.That(array.Count == toAddElements.Length,
                    string.Format("La cantidad de elementos no coincide!\nSe añadieron {0} elementos, pero el conteo es de {1} elementos", toAddElements.Length, array.Count));
    }

    [Test]
    [TestCase(new int[] { 4, 5, 6 }, 0, 2)]
    [TestCase(new int[] { 4, 5, 6 }, 1, 2)]
    [TestCase(new int[] { 4, 5, 6 }, 2, 2)]
    [TestCase(new int[] { 4, 5, 6 }, 3, 2)]
    [TestCase(new int[] { 4, 5, 6 }, 4, 2)]
    [TestCase(new int[] { 4, 5, 6 }, 5, 2)]
    [TestCase(new int[] { 4, 5, 6 }, 6, 2)]
    [TestCase(new int[] { 4, 5, 6 }, 7, 2)]
    public void Test_DinamicArray_InsertAtIndex(
        int[] initialValues,
        int index,
        int value)
    {
        //Arrange
        var array = new DinamicArray<int>();
        for (int i = 0; i < initialValues.Length; i++)
            array.Add(initialValues[i]);

        //Act
        array.Insert(value, index);

        //Assert.That()
        Assert.That(array[index] == value, 
            string.Format("El valor en index es incorrecto.\nEl valor correcto es {0} mientras que el guardado es {1}", value, array[index]));
    }


    //Esta funcionalidad estaría si yo no permitiera insertar elementos x fuera del límite que tiene el array actualmente.
    //[Test]
    //[TestCase( 2, 2 )]
    //[TestCase(0, 2)]
    //public void Test_DinamicArray_InsertAtIndex_ThrowsOutOfRangeException(
    //    int index,
    //    int value)
    //{
    //    //Arrange
    //    var array = new DinamicArray<int>();

    //    //Act and Assert
    //    Assert.Throws<System.IndexOutOfRangeException>(() => array.Insert(value, index));
    //}

    [Test]
    [TestCase(4,0)]
    [TestCase(5,1)]
    [TestCase(6,2)]
    public void Test_DinamicArray_IndexOf_IndexExists(
        int value,
        int index)
    {
        //Arrange
        var array = new DinamicArray<int>();
        int[] initialElements = { 4, 5, 6 };
        for (int i = 0; i < initialElements.Length; i++)
            array.Add(initialElements[i]);

        //Assert
        Assert.That(array.IndexOf(value) == index, 
            string.Format("Los índices no coínciden"));
    }
    [Test]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(7)]
    public void Test_DinamicArray_IndexOf_IndexDoesNotExists(int value)
    {
        //Arrange
        var array = new DinamicArray<int>();
        int[] initialElements = { 4, 5, 6 };
        for (int i = 0; i < initialElements.Length; i++)
            array.Add(initialElements[i]);

        //Assert
        Assert.That(array.IndexOf(value) == -1);
    }

    [Test]
    [TestCase(4, true)]
    [TestCase(5, true)]
    [TestCase(6, true)]
    [TestCase(7, false)]
    [TestCase(0, false)]
    public void Test_DinamicArray_Contains(
        int Element,
        bool expectedResult)
    {
        //Arrange
        var array = new DinamicArray<int>();
        int[] initialElements = { 4, 5, 6 };
        for (int i = 0; i < initialElements.Length; i++)
            array.Add(initialElements[i]);

        Assert.That(array.Contains(Element) == expectedResult, 
            "El resultado no es el esperado.");
    }

    //[Test]
    //[TestCase(new int[] { 4 }, 4, new int[] {})]
    //[TestCase(new int[] { 4, 5, 6 }, 4, new int[] { 5, 6})]
    //[TestCase(new int[] { 4, 5, 6 }, 5, new int[] { 4, 6})]
    //[TestCase(new int[] { 4, 5, 6 }, 6, new int[] { 4, 5})]
    //public void Test_DinamicArray_Remove_SingleElement(
    //    int[] initialElements,
    //    int ElementToRemove,
    //    int[] expectedResult)
    //{
    //    //Arrange
    //    var array1 = new DinamicArray<int>();
    //    for (int i = 0; i < initialElements.Length; i++)
    //        array1.Add(initialElements[i]);

    //    //Act
    //    array1.Remove(ElementToRemove);
    //    bool countIsCorrect = array1.Count == expectedResult.Length;
    //    bool AllElementsAreCorrect = array1.Count == 0;
    //    for (int i = 0; i < array1.Count; i++)
    //    {
    //        AllElementsAreCorrect = expectedResult[i] == array1[i];
    //        if (!AllElementsAreCorrect) break;
    //    }

    //    //Assert
    //    Assert.That(countIsCorrect && AllElementsAreCorrect,
    //        string.Format("El numero de elementos {0} y todos los valores {1}"
    //                    , countIsCorrect ? "coincide": "no coincide",
    //                    AllElementsAreCorrect ? "coinciden" : "no coinciden"));
    //}

    [Test]
    [TestCase(new int[] { 4, 5 }, new int[] { 4,5 }, new int[] { })]
    [TestCase(new int[] { 4, 5, 6 }, new int[] { 4,5,6 }, new int[] { })]
    [TestCase(new int[] { 4, 5, 6 }, new int[] { 4, 6 }, new int[] { 5 })]
    [TestCase(new int[] { 4, 5, 6 }, new int[] { 5, 6 }, new int[] { 4 })]
    [TestCase(new int[] { 4, 5, 6 }, new int[] { 4, 5 }, new int[] { 6 })]
    public void Test_DinamicArray_Remove_MultipleElements(
        int[] initialElements,
        int[] ElementsToRemove,
        int[] expectedResult)
    {
        //Arrange
        var array1 = new DinamicArray<int>();
        for (int i = 0; i < initialElements.Length; i++)
            array1.Add(initialElements[i]);

        //Act
        for (int i = 0; i < ElementsToRemove.Length; i++)
            array1.Remove(ElementsToRemove[i]);
        bool countIsCorrect = array1.Count == expectedResult.Length;
        bool AllElementsAreCorrect = array1.Count == 0;
        for (int i = 0; i < array1.Count; i++)
        {
            AllElementsAreCorrect = expectedResult[i] == array1[i];
            if (!AllElementsAreCorrect) break;
        }

        //Assert
        Assert.That(countIsCorrect && AllElementsAreCorrect,
            string.Format("El numero de elementos {0} y todos los valores {1}"
                        , countIsCorrect ? "coincide" : "no coincide",
                        AllElementsAreCorrect ? "coinciden" : "no coinciden"));
    }

    [Test]
    [TestCase(new int[] { 4 }, 0, new int[] {})]
    [TestCase(new int[] { 4, 5, 6 }, 0, new int[] { 5, 6 })]
    [TestCase(new int[] { 4, 5, 6 }, 1, new int[] { 4, 6 })]
    [TestCase(new int[] { 4, 5, 6 }, 2, new int[] { 4, 5 })]
    //[TestCase(new int[] { 4, 5, 6 }, 3, new int[] { 4, 5, 6 })]
    //[TestCase(new int[] { 4, 5, 6 }, 4, new int[] { 4, 5, 6 })]
    public void Test_DinamicArray_RemoveAt(
        int[] initialElements,
        int indexToRemove,
        int[] expectedResult)
    {
        //Arrange
        var array1 = new DinamicArray<int>();
        for (int i = 0; i < initialElements.Length; i++)
            array1.Add(initialElements[i]);

        //Act
        array1.RemoveAt(indexToRemove);


        //Assert
        bool countIsCorrect = array1.Count == expectedResult.Length;

        bool elementsAreFine = false;
        if (expectedResult.Length > 0)
        {
            for (int i = 0; i < array1.Count; i++)
            {
                elementsAreFine = array1[i] == expectedResult[i];
                if (!elementsAreFine) break;
            }
        }
        else
            elementsAreFine = array1.Count == 0;

        Assert.That(countIsCorrect && elementsAreFine);
    }

    [Test]
    [TestCase(-1)]
    [TestCase(2)]
    [TestCase(3)]
    public void Test_DinamicArray_RemoveAt_TrowsInvalidIndexException(int index)
    {
        DinamicArray<int> array1 = new DinamicArray<int>();
        array1.Add(1);
        array1.Add(2);

        Assert.Throws<System.IndexOutOfRangeException>(() => array1.RemoveAt(index));
    }

    [Test]
    public void Test_DinamicArray_Clear()
    {
        DinamicArray<int> array1 = new DinamicArray<int>();
        array1.Add(2);
        array1.Add(3);
        array1.Add(4);
        array1.Add(5);

        array1.Clear();

        Assert.That(array1.Count == 0);
    }

    [Test]
    [TestCase(new int[] { 4,5,6 }, new int[] { 1, 2, 3 }, new int[] { 1, 2, 3 })]
    [TestCase(new int[] { 3, 5, 10 }, new int[] { 10, 4, 3 }, new int[] { 10, 4, 3 })]
    [TestCase(new int[] { 6, 5, 5 }, new int[] { 3, 2, 6 }, new int[] { 3, 2, 6 })]
    public void Test_DinamicArray_Clear_AddSecondaryValues(
        int [] initialValues,
        int [] secondaryValues,
        int [] expectedResult)
    {
        DinamicArray<int> array1 = new DinamicArray<int>();
        for (int i = 0; i < initialValues.Length; i++)
        {
            array1.Add(initialValues[i]);
        }

        //Act
        array1.Clear();

        for (int i = 0; i < secondaryValues.Length; i++)
        {
            array1.Add(secondaryValues[i]);
        }

        //Assert
        bool ammountOfElementsIsCorrect = array1.Count == expectedResult.Length;
        bool AllElementsAreCorrect = false;
        for (int i = 0; i < array1.Count; i++)
        {
            AllElementsAreCorrect = array1[i] == expectedResult[i];
        }

        Assert.That(ammountOfElementsIsCorrect && AllElementsAreCorrect);
    }
}
