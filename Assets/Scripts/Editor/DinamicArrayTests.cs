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
        {
            array.Add(initialValues[i]);
        }

        //Act
        array.Insert(value, index);

        //Assert

        //Assert.That()
        Assert.That(array[index] == value, 
            string.Format("El valor en index es incorrecto.\nEl valor correcto es {0} mientras que el guardado es {1}", value, array[index]));
    }
}
