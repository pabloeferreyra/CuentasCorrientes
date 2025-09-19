$(function () {
    // Configuración centralizada
    const config = {
        selectors: {
            tableContainer: '#movementsTableContainer',
            modal: '#CreateMovementModal',
            sortLink: '.sort-link',
            deleteLink: '.delete-link'
        },
        urls: {
            movementsIndex: '/Movements/Index/'
        },
        messages: {
            deleteConfirm: '¿Estás seguro de que quieres eliminar este movimiento?',
            deleteError: 'Error al eliminar el movimiento.',
            createError: 'Error al crear el movimiento.'
        }
    };


    window.showToast = function (message, type = 'success') {
        let toastElement = document.getElementById(type + 'Toast');

        if (!toastElement) {
            createToastElement(type);
            toastElement = document.getElementById(type + 'Toast');
        }

        if (!toastElement) {
            alert(message); 
            return;
        }

        const messageElement = toastElement.querySelector('.toast-message');
        if (messageElement) {
            messageElement.textContent = message;
        }

        if (typeof bootstrap !== 'undefined' && bootstrap.Toast) {
            const toast = new bootstrap.Toast(toastElement, {
                autohide: true,
                delay: 4000
            });
            toast.show();
        } else {
            alert(message);
        }
    };

    function createToastElement(type) {
        const container = document.querySelector('.toast-container');
        if (!container) {
            console.error('Toast container not found');
            return;
        }

        const bgClass = type === 'success' ? 'bg-success' : 'bg-danger';
        const icon = type === 'success' ? 'bi-check-circle' : 'bi-exclamation-triangle';

        const toastHTML = `
            <div id="${type}Toast" class="toast align-items-center text-white ${bgClass} border-0" role="alert" aria-live="assertive" aria-atomic="true">
                <div class="d-flex">
                    <div class="toast-body">
                        <i class="bi ${icon} me-2"></i>
                        <span class="toast-message"></span>
                    </div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
            </div>
        `;

        container.insertAdjacentHTML('beforeend', toastHTML);
    }

    window.showLoadingIndicator = function (show) {
        const indicator = document.getElementById('loadingIndicator');
        const container = document.getElementById('movementsTableContainer');

        if (show) {
            if (indicator) indicator.style.display = 'block';
            if (container) container.style.opacity = '0.5';
        } else {
            if (indicator) indicator.style.display = 'none';
            if (container) container.style.opacity = '1';
        }
    };

    const showMessage = (message, type = 'success') => {
        if (typeof window.showToast === 'function') {
            window.showToast(message, type);
        } else {
            if (type === 'error') {
                console.error(message);
                alert('Error: ' + message);
            } else {
                console.log(message);
                alert(message);
            }
        }
    };

    function bindSortLinks() {
        $(config.selectors.sortLink).off('click').on('click', handleSortClick);
    }

    function handleSortClick(e) {
        e.preventDefault();
        const url = $(this).attr('href');

        showLoadingIndicator(true);

        $.get(url)
            .done(function (data) {
                $(config.selectors.tableContainer).html(data);
                bindSortLinks();
            })
            .fail(function () {
                showMessage('Error al cargar los datos.', 'error');
            })
            .always(function () {
                showLoadingIndicator(false);
            });
    }

    function handleDeleteClick(e) {
        e.preventDefault();

        if (!confirm(config.messages.deleteConfirm)) {
            return;
        }

        const url = $(this).attr('href');
        const currentAccountId = $(this).data('current-account');

        $.ajax({
            type: 'POST',
            url: url,
            beforeSend: function () {
                showLoadingIndicator(true);
            }
        })
            .done(function () {
                reloadTable(currentAccountId);
                showMessage('Movimiento eliminado correctamente.', 'success');
            })
            .fail(function () {
                showMessage(config.messages.deleteError, 'error');
            })
            .always(function () {
                showLoadingIndicator(false);
            });
    }

    window.showDeleteConfirmation = function (id, currentAccount) {
        if (!confirm(config.messages.deleteConfirm)) {
            return;
        }


        const form = $(`#deleteForm-${id}`);

        $.ajax({
            type: 'POST',
            url: form.attr('action'),
            data: form.serialize(),
            beforeSend: function () {
                showLoadingIndicator(true);
            }
        })
            .done(function () {
                reloadTable(currentAccount);
                showMessage('Movimiento eliminado correctamente.', 'success');
            })
            .fail(function () {
                showMessage(config.messages.deleteError, 'error');
            })
            .always(function () {
                showLoadingIndicator(false);
            });
    };

    function initializeModal() {
        $(config.selectors.modal)
            .on('shown.bs.modal', handleModalShown)
            .on('submit', 'form', handleFormSubmit);
    }

    function handleModalShown() {
        const form = $(this).find('form')[0];
        if (form) {
            form.reset();
            $(form).find('input:visible:first').focus();
        }
    }

    function handleFormSubmit(e) {
        e.preventDefault();

        const form = $(this);
        const submitButton = form.find('[type="submit"]');

        submitButton.prop('disabled', true);

        $.ajax({
            type: 'POST',
            url: form.attr('action'),
            data: form.serialize()
        })
            .done(function (response) {
                if (response.success) {
                    if (response.redirectUrl) {
                        window.location.href = response.redirectUrl;
                    } else {
                        $(config.selectors.modal).modal('hide');
                        reloadTable();
                        showMessage('Movimiento creado correctamente.', 'success');
                    }
                } else {
                    showMessage(response.error || 'Error desconocido.');
                }
            })
            .fail(function (xhr) {
                showMessage(`${config.messages.createError}: ${xhr.responseText}`, 'error');
            })
            .always(function () {
                submitButton.prop('disabled', false);
            });
    }

    function reloadTable(id) {
        const url = id ? `${config.urls.movementsIndex}${id}` : window.location.pathname;

        showLoadingIndicator(true);

        $.get(url)
            .done(function (data) {
                $(config.selectors.tableContainer).html(data);
                bindSortLinks();
            })
            .fail(function () {
                showMessage('Error al recargar la tabla.', 'error');
            })
            .always(function () {
                showLoadingIndicator(false);
            });
    }

    function showLoadingIndicator(show) {
        if (typeof window.showLoadingIndicator === 'function') {
            window.showLoadingIndicator(show);
        } else {
            // Fallback básico
            if (show) {
                $(config.selectors.tableContainer).addClass('loading');
            } else {
                $(config.selectors.tableContainer).removeClass('loading');
            }
        }
    }

    function bindDynamicEvents() {
        $(document)
            .on('click', config.selectors.deleteLink, handleDeleteClick)
            .on('click', config.selectors.sortLink, handleSortClick);
    }

    function initialize() {
        bindSortLinks();
        initializeModal();
        bindDynamicEvents();
    }

    // Ejecutar inicialización
    initialize();
});