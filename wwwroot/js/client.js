$(function () {
    let originalTableRows = [];
    let filteredTableRows = [];
    let currentPage = 1;
    let itemsPerPage = 10; 
    let totalPages = 1;

    function initializeTableData() {
        originalTableRows = [];
        $('#clientTableContainer tbody tr').each(function () {
            const row = $(this);

            let searchText = '';
            row.find('td').not(':last').each(function () {
                searchText += $(this).text().toLowerCase() + ' ';
            });

            const rowData = {
                element: row.clone(true), 
                searchText: searchText.trim(),
                name: row.find('td:nth-child(1)').text().trim(),
                surname: row.find('td:nth-child(2)').text().trim(),
                email: row.find('td:nth-child(3)').text().trim(),
                cuit: row.find('td:nth-child(4)').text().trim(),
                companyName: row.find('td:nth-child(5)').text().trim(),
                clientType: row.find('td:nth-child(6)').text().trim()
            };
            originalTableRows.push(rowData);
        });
    }

    function performSearch(searchTerm) {
        const searchLower = searchTerm.toLowerCase().trim();

        if (searchLower === '') {
            filteredTableRows = [...originalTableRows];
        } else {
            filteredTableRows = originalTableRows.filter(function (rowData) {
                return rowData.searchText.includes(searchLower) ||
                    rowData.name.toLowerCase().includes(searchLower) ||
                    rowData.surname.toLowerCase().includes(searchLower) ||
                    rowData.email.toLowerCase().includes(searchLower) ||
                    rowData.cuit.includes(searchLower) ||
                    rowData.companyName.toLowerCase().includes(searchLower) ||
                    rowData.clientType.toLowerCase().includes(searchLower);
            });
        }

        currentPage = 1;
        updatePagination();
        displayCurrentPage(searchTerm);
    }

    function displayCurrentPage(searchTerm = '') {
        const tbody = $('#clientTableContainer tbody');
        const startIndex = (currentPage - 1) * itemsPerPage;
        const endIndex = startIndex + itemsPerPage;
        const pageRows = filteredTableRows.slice(startIndex, endIndex);

        tbody.empty();

        if (pageRows.length === 0) {
            let message = filteredTableRows.length === 0 ?
                `No se encontraron clientes que coincidan con "<strong>${searchTerm}</strong>"` :
                'No hay más resultados en esta página';

            tbody.append(`
                <tr>
                    <td colspan="100%" class="text-center text-muted py-4">
                        <i class="fas fa-search"></i><br>
                        ${message}
                    </td>
                </tr>
            `);
        } else {
            pageRows.forEach(function (rowData) {
                let displayRow = rowData.element.clone(true);
                if (searchTerm.trim() !== '') {
                    displayRow = highlightSearchTerm(displayRow, searchTerm);
                }
                tbody.append(displayRow);
            });
        }

        updateSearchResults();
        updatePaginationControls();
    }

    function updatePagination() {
        totalPages = Math.ceil(filteredTableRows.length / itemsPerPage);
        if (currentPage > totalPages && totalPages > 0) {
            currentPage = totalPages;
        }
        if (currentPage < 1) {
            currentPage = 1;
        }
    }

    function updatePaginationControls() {
        let paginationHtml = '';

        if (totalPages > 0) {
            paginationHtml = `
                <nav aria-label="Navegación de páginas de clientes">
                    <ul class="pagination justify-content-center mt-3">
                        <li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
                            <button class="page-link" data-page="${currentPage - 1}" ${currentPage === 1 ? 'disabled' : ''}>
                                <i class="fas fa-chevron-left"></i> <<
                            </button>
                        </li>
            `;

            let startPage = Math.max(1, currentPage - 2);
            let endPage = Math.min(totalPages, currentPage + 2);

            if (currentPage <= 3) {
                endPage = Math.min(5, totalPages);
            }
            if (currentPage > totalPages - 3) {
                startPage = Math.max(1, totalPages - 4);
            }

            if (startPage > 1) {
                paginationHtml += `
                    <li class="page-item">
                        <button class="page-link" data-page="1">1</button>
                    </li>
                `;
                if (startPage > 2) {
                    paginationHtml += `
                        <li class="page-item disabled">
                            <span class="page-link">...</span>
                        </li>
                    `;
                }
            }

            for (let i = startPage; i <= endPage; i++) {
                paginationHtml += `
                    <li class="page-item ${i === currentPage ? 'active' : ''}">
                        <button class="page-link" data-page="${i}">${i}</button>
                    </li>
                `;
            }

            if (endPage < totalPages) {
                if (endPage < totalPages - 1) {
                    paginationHtml += `
                        <li class="page-item disabled">
                            <span class="page-link">...</span>
                        </li>
                    `;
                }
                paginationHtml += `
                    <li class="page-item">
                        <button class="page-link" data-page="${totalPages}">${totalPages}</button>
                    </li>
                `;
            }

            paginationHtml += `
                        <li class="page-item ${currentPage === totalPages ? 'disabled' : ''}">
                            <button class="page-link" data-page="${currentPage + 1}" ${currentPage === totalPages ? 'disabled' : ''}>
                                >> <i class="fas fa-chevron-right"></i>
                            </button>
                        </li>
                    </ul>
                </nav>
                
                <!-- Selector de elementos por página -->
                <div class="row mt-2">
                    <div class="col-md-6">
                        <div class="d-flex align-items-center">
                            <label for="itemsPerPageSelect" class="form-label me-2 mb-0">Mostrar:</label>
                            <select id="itemsPerPageSelect" class="form-select" style="width: auto;">
                                <option value="5" ${itemsPerPage === 5 ? 'selected' : ''}>5</option>
                                <option value="10" ${itemsPerPage === 10 ? 'selected' : ''}>10</option>
                                <option value="25" ${itemsPerPage === 25 ? 'selected' : ''}>25</option>
                                <option value="50" ${itemsPerPage === 50 ? 'selected' : ''}>50</option>
                                <option value="100" ${itemsPerPage === 100 ? 'selected' : ''}>100</option>
                            </select>
                            <span class="ms-2">elementos por página</span>
                        </div>
                    </div>
                    <div class="col-md-6">
                        <div class="text-end">
                            <small class="text-muted">
                                Página ${currentPage} de ${totalPages} 
                                (${((currentPage - 1) * itemsPerPage) + 1}-${Math.min(currentPage * itemsPerPage, filteredTableRows.length)} de ${filteredTableRows.length} elementos)
                            </small>
                        </div>
                    </div>
                </div>
            `;
        }

        let paginationContainer = $('#paginationContainer');
        if (paginationContainer.length === 0) {
            $('#clientTableContainer').after('<div id="paginationContainer"></div>');
            paginationContainer = $('#paginationContainer');
        }
        paginationContainer.html(paginationHtml);
    }
    function highlightSearchTerm(row, searchTerm) {
        if (searchTerm.trim() === '') return row;

        const regex = new RegExp(`(${searchTerm.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')})`, 'gi');
    function highlightSearchTerm(row, searchTerm) {
                if (searchTerm.trim() === '') return row;

                const regex = new RegExp(`(${searchTerm.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')})`, 'gi');
                row.find('td').not(':last').each(function () {
                    const cell = $(this);
                    const html = cell.html();
                    const highlightedHtml = html.replace(regex, '<mark class="bg-warning">$1</mark>');
                    cell.html(highlightedHtml);
                });
                return row;
            }
        row.find('td').not(':last').each(function () {
                const cell = $(this);

                if (cell.find('*').length > 0) {
                    highlightTextNodes(cell, regex);
                } else {
                    const html = cell.html();
                    const highlightedHtml = html.replace(regex, '<mark class="bg-warning">$1</mark>');
                    cell.html(highlightedHtml);
                }
            });
        return row;
    }

    function highlightTextNodes(element, regex) {
        element.contents().each(function () {
            if (this.nodeType === 3) { 
                const text = $(this).text();
                if (regex.test(text)) {
                    const highlightedText = text.replace(regex, '<mark class="bg-warning">$1</mark>');
                    $(this).replaceWith(highlightedText);
                }
            } else if (this.nodeType === 1) {
                const tagName = this.tagName.toLowerCase();
                if (tagName !== 'mark') { 
                    highlightTextNodes($(this), regex);
                }
            }
        });
    }
    function updateSearchResults() {
        const resultsSpan = $('#searchResults');
        const startIndex = (currentPage - 1) * itemsPerPage + 1;
        const endIndex = Math.min(currentPage * itemsPerPage, filteredTableRows.length);

        if (filteredTableRows.length === originalTableRows.length) {
            if (totalPages > 1) {
                resultsSpan.text(`Mostrando ${startIndex}-${endIndex} clientes`);
            } else {
                resultsSpan.text(`Mostrando todos los clientes`);
            }
        } else {
            if (totalPages > 1) {
                resultsSpan.text(`Mostrando ${startIndex}-${endIndex} clientes encontrados `);
            } else {
                resultsSpan.text(`Mostrando ${filteredTableRows.length} clientes`);
            }
        }
    }

    $('#searchInput').on('input keyup', function () {
        const searchTerm = $(this).val();
        performSearch(searchTerm);
    });

    $('#clearSearch').on('click', function () {
        $('#searchInput').val('').focus();
        performSearch('');
    });

    $(document).on('click', '.page-link[data-page]', function (e) {
        e.preventDefault();
        const page = parseInt($(this).data('page'));
        if (page !== currentPage && page >= 1 && page <= totalPages) {
            currentPage = page;
            const currentSearch = $('#searchInput').val();
            displayCurrentPage(currentSearch);
        }
    });

    $(document).on('change', '#itemsPerPageSelect', function () {
        itemsPerPage = parseInt($(this).val());
        currentPage = 1; 
        updatePagination();
        const currentSearch = $('#searchInput').val();
        displayCurrentPage(currentSearch);
    });

    $('#searchForm').on('submit', function (e) {
        e.preventDefault();
        return false;
    });

    function reloadTable() {
        const indexUrl = window.clientIndexUrl || '/Client/Index';
        $.get(indexUrl, function (data) {
            $('#clientTableContainer').html(data);
            initializeTableData();
            bindSortLinks();

            currentPage = 1;
            filteredTableRows = [...originalTableRows];
            updatePagination();

            const currentSearch = $('#searchInput').val();
            if (currentSearch) {
                performSearch(currentSearch);
            } else {
                displayCurrentPage();
            }
        });
    }
    function bindSortLinks() {
        $('.sort-link').off('click').on('click', function (e) {
            e.preventDefault();
            const url = $(this).attr('href');
            const currentSearch = $('#searchInput').val();

            $.get(url, function (data) {
                $('#clientTableContainer').html(data);
                initializeTableData();
                bindSortLinks();

                currentPage = 1;
                filteredTableRows = [...originalTableRows];
                updatePagination();

                if (currentSearch) {
                    $('#searchInput').val(currentSearch);
                    performSearch(currentSearch);
                } else {
                    displayCurrentPage();
                }
            });
        });
    }

    initializeTableData();
    bindSortLinks();

    filteredTableRows = [...originalTableRows];
    updatePagination();
    displayCurrentPage();

    $(document).on('submit', '#createClientForm', function (e) {
        e.preventDefault();
        var form = $(this);
        $.ajax({
            type: 'POST',
            url: form.attr('action'),
            data: form.serialize(),
            success: function () {
                $('#CreateClientModal').modal('hide');
                reloadTable();
            },
            error: function () {
                alert('Error al crear el cliente.');
            }
        });
    });

    $('#editClientModal').on('show.bs.modal', function (event) {
        var button = $(event.relatedTarget);
        var id = button.data('id');
        const editUrl = window.clientEditUrl || '/Client/Edit';
        var url = editUrl + '/' + id;
        $.get(url, function (data) {
            $('#editClientModal .modal-body').html(data);
        });
    });

    $(document).on('submit', '#editClientForm', function (e) {
        e.preventDefault();
        var form = $(this);
        $.ajax({
            type: 'POST',
            url: form.attr('action'),
            data: form.serialize(),
            success: function () {
                $('#editClientModal').modal('hide');
                reloadTable();
            },
            error: function () {
                alert('Error al editar el cliente.');
            }
        });
    });

    window.showDeleteConfirmation = function (id) {
        if (confirm('¿Estás seguro de que deseas eliminar este cliente?')) {
            $.ajax({
                type: 'POST',
                url: $('#deleteForm-' + id).attr('action'),
                data: $('#deleteForm-' + id).serialize(),
                success: function () {
                    reloadTable();
                },
                error: function () {
                    alert('Error al eliminar el cliente.');
                }
            });
        }
    };

    $(document).on('focusout', '#Cuit', function () {
        var value = $(this).val().replace(/\D/g, '');
        if (value.length === 11) {
            var formatted = value.substr(0, 2) + '-' + value.substr(2, 8) + '-' + value.substr(10, 1);
            $(this).val(formatted);
            $('#cuitFormatted').text('');
        } else {
            $('#cuitFormatted').text('CUIT inválido');
        }
    });

    $(document).on('focusout', '#Email', function () {
        var email = $(this).val().trim();
        var emailSpan = $('#emailFormatted');
        var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

        if (!emailRegex.test(email)) {
            emailSpan.text('Correo inválido');
        }
        else {
            emailSpan.text('');
        }
    });

    $(document).on('focusout', '#Name', function () {
        var name = $(this).val().trim();
        var nameSpan = $('#nameFormatted');

        if (name.length === 0) {
            nameSpan.text('El nombre es obligatorio');
        } else {
            var formatted = name.split(' ').map(function (word) {
                return word.charAt(0).toUpperCase() + word.slice(1).toLowerCase();
            }).join(' ');
            $(this).val(formatted);
            nameSpan.text('');
        }
    });

    $(document).on('focusout', '#Surname', function () {
        var name = $(this).val().trim();
        var nameSpan = $('#surnameFormatted');

        if (name.length === 0) {
            nameSpan.text('El apellido es obligatorio');
        } else {
            var formatted = name.split(' ').map(function (word) {
                return word.charAt(0).toUpperCase() + word.slice(1).toLowerCase();
            }).join(' ');
            $(this).val(formatted);
            nameSpan.text('');
        }
    });

    $(document).on('focusout', '#CompanyName', function () {
        var name = $(this).val().trim();
        if (name.length === 0) {
        } else {
            var formatted = name.split(' ').map(function (word) {
                return word.charAt(0).toUpperCase() + word.slice(1).toLowerCase();
            }).join(' ');
            $(this).val(formatted);
        }
    });
});