$(function () {
    function bindSortLinks() {
        $('.sort-link').off('click').on('click', function (e) {
            e.preventDefault();
            var url = $(this).attr('href');
            $.get(url, function (data) {
                $('#currentAccountTableContainer').html(data);
                bindSortLinks();
            });
        });
    }
    bindSortLinks();

    // Crear cuenta corriente
    $('#CreateCurrentAccountModal').on('shown.bs.modal', function () {
        $(this).find('form')[0]?.reset();
    });
    $('#CreateCurrentAccountModal').on('submit', 'form', function (e) {
        e.preventDefault();
        var form = $(this);
        $.ajax({
            type: 'POST',
            url: '/CurrentAccount/Create',
            data: form.serialize(),
            success: function () {
                $('#CreateCurrentAccountModal').modal('hide');
                reloadTable();
            },
            error: function () {
                alert('Error al crear la cuenta corriente.');
            }
        });
    });

    // Editar cuenta corriente
    $('#currentAccountTableContainer').on('click', '.edit-link', function (e) {
        e.preventDefault();
        var url = $(this).attr('href');
        $.get(url, function (data) {
            $('#editCurrentAccountModal .modal-body').html(data);
            $('#editCurrentAccountModal').modal('show');
        });
    });
    $('#editCurrentAccountModal').on('submit', 'form', function (e) {
        e.preventDefault();
        var form = $(this);
        $.ajax({
            type: 'POST',
            url: form.attr('action'),
            data: form.serialize(),
            success: function () {
                $('#editCurrentAccountModal').modal('hide');
                reloadTable();
            },
            error: function () {
                alert('Error al editar la cuenta corriente.');
            }
        });
    });

    // Eliminar cuenta corriente
    $('#currentAccountTableContainer').on('click', '.delete-link', function (e) {
        e.preventDefault();
        if (confirm('¿Estás seguro de que quieres eliminar esta cuenta corriente?')) {
            var url = $(this).attr('href');
            $.ajax({
                type: 'POST',
                url: url,
                success: function () {
                    reloadTable();
                },
                error: function () {
                    alert('Error al eliminar la cuenta corriente.');
                }
            });
        }
    });

    // Recargar tabla
    function reloadTable() {
        var url = '@Url.Action("Index")';
        $.get(url, function (data) {
            $('#currentAccountTableContainer').html(data);
            bindSortLinks();
        });
    }

    // Ordenar tabla
    $('#currentAccountTableContainer').on('click', '.sort-link', function (e) {
        e.preventDefault();
        var url = $(this).attr('href');
        $.get(url, function (data) {
            $('#currentAccountTableContainer').html(data);
            bindSortLinks();
        });
    });
});