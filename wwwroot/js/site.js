// ========== SISTEMA DE TOAST UNIVERSAL ==========
// Agregar este código a tu site.js

(function () {
    'use strict';

    // Configuración del sistema de toast
    const toastConfig = {
        types: {
            success: {
                bgClass: 'bg-success',
                icon: 'bi-check-circle',
                title: 'Éxito'
            },
            error: {
                bgClass: 'bg-danger',
                icon: 'bi-exclamation-triangle',
                title: 'Error'
            },
            warning: {
                bgClass: 'bg-warning',
                icon: 'bi-exclamation-triangle',
                title: 'Advertencia',
                textClass: 'text-dark'
            },
            info: {
                bgClass: 'bg-info',
                icon: 'bi-info-circle',
                title: 'Información'
            }
        },
        defaultOptions: {
            autohide: true,
            delay: 4000,
            animation: true
        },
        container: {
            id: 'toast-container',
            classes: 'toast-container position-fixed bottom-0 end-0 p-3',
            style: 'z-index: 1055;'
        }
    };

    // Crear contenedor de toast si no existe
    function ensureToastContainer() {
        let container = document.getElementById(toastConfig.container.id);

        if (!container) {
            container = document.createElement('div');
            container.id = toastConfig.container.id;
            container.className = toastConfig.container.classes;
            container.style.cssText = toastConfig.container.style;
            document.body.appendChild(container);
        }

        return container;
    }

    // Crear elemento toast
    function createToastElement(message, type = 'success', options = {}) {
        const typeConfig = toastConfig.types[type] || toastConfig.types.success;
        const toastId = 'toast-' + Date.now() + '-' + Math.random().toString(36).substr(2, 9);
        const textClass = typeConfig.textClass || 'text-white';
        const closeClass = textClass === 'text-dark' ? 'btn-close' : 'btn-close-white';

        const toastHTML = `
            <div id="${toastId}" 
                 class="toast align-items-center ${textClass} ${typeConfig.bgClass} border-0" 
                 role="alert" 
                 aria-live="assertive" 
                 aria-atomic="true">
                <div class="d-flex">
                    <div class="toast-body">
                        <i class="bi ${typeConfig.icon} me-2"></i>
                        ${options.showTitle && typeConfig.title ? `<strong>${typeConfig.title}:</strong> ` : ''}
                        <span class="toast-message">${message}</span>
                    </div>
                    <button type="button" 
                            class="btn-close ${closeClass} me-2 m-auto" 
                            data-bs-dismiss="toast" 
                            aria-label="Close">
                    </button>
                </div>
            </div>
        `;

        const container = ensureToastContainer();
        container.insertAdjacentHTML('beforeend', toastHTML);

        return document.getElementById(toastId);
    }

    // Función principal de toast
    window.showToast = function (message, type = 'success', options = {}) {
        // Validar parámetros
        if (!message) {
            console.warn('showToast: mensaje requerido');
            return;
        }

        // Combinar opciones
        const finalOptions = { ...toastConfig.defaultOptions, ...options };

        try {
            // Crear elemento toast
            const toastElement = createToastElement(message, type, finalOptions);

            if (!toastElement) {
                throw new Error('No se pudo crear el elemento toast');
            }

            // Verificar si Bootstrap está disponible
            if (typeof bootstrap === 'undefined' || !bootstrap.Toast) {
                console.warn('Bootstrap Toast no está disponible, usando fallback');
                fallbackToast(message, type);
                return;
            }

            // Crear instancia de Bootstrap Toast
            const bsToast = new bootstrap.Toast(toastElement, {
                autohide: finalOptions.autohide,
                delay: finalOptions.delay,
                animation: finalOptions.animation
            });

            // Eventos del toast
            toastElement.addEventListener('hidden.bs.toast', function () {
                if (this.parentNode) {
                    this.parentNode.removeChild(this);
                }
            });

            // Mostrar toast
            bsToast.show();

            // Log para debugging (opcional)
            if (finalOptions.debug) {
                console.log('Toast mostrado:', { message, type, options: finalOptions });
            }

        } catch (error) {
            console.error('Error al mostrar toast:', error);
            fallbackToast(message, type);
        }
    };

    // Fallback si Bootstrap no está disponible
    function fallbackToast(message, type) {
        const typeConfig = toastConfig.types[type] || toastConfig.types.success;
        const title = typeConfig.title;

        if (type === 'error') {
            console.error(`${title}: ${message}`);
        } else {
            console.log(`${title}: ${message}`);
        }

        alert(`${title}: ${message}`);
    }

    // Funciones de conveniencia
    window.showSuccess = function (message, options = {}) {
        return showToast(message, 'success', options);
    };

    window.showError = function (message, options = {}) {
        return showToast(message, 'error', options);
    };

    window.showWarning = function (message, options = {}) {
        return showToast(message, 'warning', options);
    };

    window.showInfo = function (message, options = {}) {
        return showToast(message, 'info', options);
    };

    // Función para configurar toast globalmente
    window.configureToast = function (newConfig) {
        Object.assign(toastConfig.defaultOptions, newConfig);
    };

    // Función para limpiar todos los toasts
    window.clearAllToasts = function () {
        const container = document.getElementById(toastConfig.container.id);
        if (container) {
            const toasts = container.querySelectorAll('.toast');
            toasts.forEach(toast => {
                if (bootstrap && bootstrap.Toast) {
                    const bsToast = bootstrap.Toast.getInstance(toast);
                    if (bsToast) {
                        bsToast.hide();
                    }
                }
                setTimeout(() => {
                    if (toast.parentNode) {
                        toast.parentNode.removeChild(toast);
                    }
                }, 150);
            });
        }
    };

    // Función de prueba
    window.testToasts = function () {
        showSuccess('¡Operación completada exitosamente!');

        setTimeout(() => showError('Ha ocurrido un error en el sistema'), 1000);
        setTimeout(() => showWarning('Esta es una advertencia importante'), 2000);
        setTimeout(() => showInfo('Información adicional disponible'), 3000);
    };

    // Inicialización cuando el DOM esté listo
    document.addEventListener('DOMContentLoaded', function () {
        // Verificar Bootstrap
        if (typeof bootstrap === 'undefined') {
            console.warn('Bootstrap no está cargado - los toasts usarán fallback');
        } else {
            console.log('Sistema de Toast Universal inicializado correctamente');
        }
    });

})();

// ========== FUNCIONES DE LOADING INDICATOR ==========

// Loading indicator universal
(function () {
    'use strict';

    // Crear overlay de loading
    function createLoadingOverlay() {
        const overlay = document.createElement('div');
        overlay.id = 'loading-overlay';
        overlay.style.cssText = `
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(0, 0, 0, 0.5);
            display: flex;
            justify-content: center;
            align-items: center;
            z-index: 9999;
            opacity: 0;
            transition: opacity 0.3s ease;
        `;

        const spinner = document.createElement('div');
        spinner.innerHTML = `
            <div class="spinner-border text-light" style="width: 3rem; height: 3rem;" role="status">
                <span class="visually-hidden">Cargando...</span>
            </div>
        `;

        overlay.appendChild(spinner);
        document.body.appendChild(overlay);

        // Forzar reflow para que funcione la transición
        overlay.offsetHeight;
        overlay.style.opacity = '1';

        return overlay;
    }

    // Loading indicator global
    window.showLoading = function (show = true, target = null) {
        if (target) {
            // Loading en elemento específico
            showElementLoading(target, show);
        } else {
            // Loading global
            showGlobalLoading(show);
        }
    };

    function showGlobalLoading(show) {
        let overlay = document.getElementById('loading-overlay');

        if (show) {
            if (!overlay) {
                overlay = createLoadingOverlay();
            }
        } else {
            if (overlay) {
                overlay.style.opacity = '0';
                setTimeout(() => {
                    if (overlay.parentNode) {
                        overlay.parentNode.removeChild(overlay);
                    }
                }, 300);
            }
        }
    }

    function showElementLoading(target, show) {
        const element = typeof target === 'string' ? document.querySelector(target) : target;

        if (!element) {
            console.warn('Elemento no encontrado para loading indicator');
            return;
        }

        if (show) {
            element.style.position = 'relative';
            element.style.opacity = '0.6';
            element.style.pointerEvents = 'none';

            const existing = element.querySelector('.element-loading-overlay');
            if (!existing) {
                const overlay = document.createElement('div');
                overlay.className = 'element-loading-overlay';
                overlay.style.cssText = `
                    position: absolute;
                    top: 0;
                    left: 0;
                    width: 100%;
                    height: 100%;
                    display: flex;
                    justify-content: center;
                    align-items: center;
                    background: rgba(255, 255, 255, 0.8);
                    z-index: 1000;
                `;

                overlay.innerHTML = `
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">Cargando...</span>
                    </div>
                `;

                element.appendChild(overlay);
            }
        } else {
            element.style.opacity = '';
            element.style.pointerEvents = '';

            const overlay = element.querySelector('.element-loading-overlay');
            if (overlay) {
                overlay.remove();
            }
        }
    }

    // Aliases para compatibilidad
    window.showLoadingIndicator = window.showLoading;

})();