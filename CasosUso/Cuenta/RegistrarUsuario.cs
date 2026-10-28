using PintagAPP.CasosUso.Interfaces;
using PintagAPP.Dominio.Reglas;
using System;
using System.Collections.Generic;
using System.Text;

namespace PintagAPP.CasosUso.Cuenta;

public class RegistrarUsuario
{
    private readonly IAuthServicio _auth;

    public RegistrarUsuario(IAuthServicio auth)
    {
        _auth = auth;
    }

    public async Task<Resultado> EjecutarAsync(string nombresCompletos, string cedula, string correo,
                                               string contrasena, bool aceptaAviso)
    {
        nombresCompletos = nombresCompletos?.Trim() ?? "";
        cedula = cedula?.Trim() ?? "";
        correo = correo?.Trim().ToLower() ?? "";

        if (nombresCompletos.Length < 5)
            return Resultado.Error("Escribe tus nombres y apellidos completos.");

        if (!ValidadorCedula.EsValida(cedula))
            return Resultado.Error("La cédula no es válida. Revisa los 10 dígitos.");

        if (!correo.Contains('@') || !correo.Contains('.'))
            return Resultado.Error("El correo electrónico no es válido.");

        if (string.IsNullOrEmpty(contrasena) || contrasena.Length < 8)
            return Resultado.Error("La contraseña debe tener al menos 8 caracteres.");

        if (!aceptaAviso)
            return Resultado.Error("Debes aceptar la Política de Privacidad para registrarte.");

        return await _auth.RegistrarAsync(correo, contrasena, cedula, nombresCompletos);
    }
}