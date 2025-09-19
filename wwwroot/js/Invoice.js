window.onload = function () {
    setTimeout(function () {
        window.print();
    }, 500);
};

window.onafterprint = function () {
    setTimeout(function () {
        window.location.href = '@Url.Action("Index", "Movements", new { id = Model.Id })';
    }, 1000);
};