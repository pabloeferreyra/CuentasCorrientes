simple sistema de gestion de cuentas corrientes

## Suscripción mensual con Mercado Pago

La aplicación permite iniciar una suscripción mensual de ARS 17.000 desde el menú **Suscripción**. El alta queda pendiente hasta que el comprador la confirme en Mercado Pago; el retorno del navegador no confirma el pago. El estado local se sincroniza mediante la notificación `subscription_preapproval`, validando el token de la notificación y consultando la suscripción con el Access Token del servidor.

Configura estos valores antes de probar:

- `MercadoPago__AccessToken`: Access Token de prueba o producción, según el entorno.
- `MercadoPago__WebhookSecret`: valor aleatorio y privado de al menos 32 caracteres para proteger el endpoint de notificaciones.
- `MercadoPago__PublicBaseUrl`: URL pública HTTPS de la aplicación, por ejemplo `https://ejemplo.com` (sin `/CCApp`; la aplicación agrega su ruta base).
- `MercadoPago__TestPayerEmail`: correo de la cuenta compradora de prueba; solo se usa con `ASPNETCORE_ENVIRONMENT=Development`. La suscripción se vincula siempre al usuario autenticado de la aplicación.

Al iniciar la suscripción, cada usuario ingresa el correo asociado a su cuenta de Mercado Pago. Este dato no se toma del correo de acceso a la aplicación, que puede ser ficticio, ni se guarda localmente. En desarrollo, `MercadoPago__TestPayerEmail` sigue teniendo prioridad.

El importe y la moneda se configuran con `MercadoPago__MonthlyAmount` y `MercadoPago__CurrencyId`. El valor predeterminado es `17000` y `ARS`. No guardes credenciales reales en `appsettings.json`; usa variables de entorno o User Secrets.

Aplica la migración `AddApplicationSubscriptions` a la base de datos antes de iniciar la integración. En el panel de Mercado Pago, crea una aplicación y usa sus credenciales del mismo entorno. La URL de notificación se envía en cada solicitud de suscripción, ya que la documentación de Mercado Pago indica que las notificaciones de Suscripciones se configuran durante la creación de la suscripción.
