simple sistema de gestion de cuentas corrientes

## Suscripción mensual con Mercado Pago

La aplicación permite iniciar una suscripción mensual de ARS 17.000 desde el menú **Suscripción**. El alta queda pendiente hasta que el comprador la confirme en Mercado Pago; el retorno del navegador no confirma el pago. El estado local se sincroniza mediante la notificación `subscription_preapproval`, validando la firma `x-signature` con el secreto de Webhooks y consultando la suscripción con el Access Token del servidor.

Configura estos valores antes de probar:

- `MercadoPago__AccessToken`: Access Token de prueba o producción, según el entorno.
- `MercadoPago__WebhookSecret`: clave secreta de Webhooks generada en la configuración de notificaciones de tu aplicación de Mercado Pago. Se usa para verificar `x-signature`; nunca se envía en la URL.
- `MercadoPago__PublicBaseUrl`: URL pública HTTPS de la aplicación, por ejemplo `https://ejemplo.com`.
- `MercadoPago__TestPayerEmail`: correo de la cuenta compradora de prueba; solo se usa con `ASPNETCORE_ENVIRONMENT=Development`. La suscripción se vincula siempre al usuario autenticado de la aplicación.

Al iniciar la suscripción, cada usuario ingresa el correo asociado a su cuenta de Mercado Pago. Este dato no se toma del correo de acceso a la aplicación, que puede ser ficticio, ni se guarda localmente. En desarrollo, `MercadoPago__TestPayerEmail` sigue teniendo prioridad.

El importe y la moneda se configuran con `MercadoPago__MonthlyAmount` y `MercadoPago__CurrencyId`. El valor predeterminado es `17000` y `ARS`. No guardes credenciales reales en `appsettings.json`; usa variables de entorno o User Secrets.

Aplica la migración `AddApplicationSubscriptions` a la base de datos antes de iniciar la integración. En el panel de Mercado Pago, crea una aplicación, configura allí la clave secreta de Webhooks y usa credenciales del mismo entorno. La URL de notificación se envía en cada solicitud de suscripción, ya que las notificaciones de Suscripciones se configuran durante su creación; la URL no contiene el secreto.
