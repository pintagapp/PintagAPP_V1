using System;
using System.Collections.Generic;
using System.Text;

namespace PintagAPP.Dominio.Reglas;

public static class ValidadorCedula
{
    public static bool EsValida(string? cedula)
    {
        if (string.IsNullOrWhiteSpace(cedula) || cedula.Length != 10 || !cedula.All(char.IsDigit))
            return false;

        int provincia = int.Parse(cedula[..2]);
        if (!((provincia >= 1 && provincia <= 24) || provincia == 30))
            return false;

        if (cedula[2] - '0' >= 6)
            return false;

        // Coeficientes 2,1,2,1,2,1,2,1,2 sobre los primeros 9 dígitos
        int suma = 0;
        for (int i = 0; i < 9; i++)
        {
            int digito = cedula[i] - '0';
            if (i % 2 == 0)
            {
                digito *= 2;
                if (digito > 9) digito -= 9;
            }
            suma += digito;
        }

        int verificador = (10 - suma % 10) % 10;
        return verificador == cedula[9] - '0';
    }
}