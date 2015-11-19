// Write your Javascript code.
var obj = $("#dragandrophandler");
obj.on('dragenter', function (e) {
    e.stopPropagation();
    e.preventDefault();
    $(this).css('border', '2px solid #CCC');
});
obj.on('dragover', function (e) {
    e.stopPropagation();
    e.preventDefault();
});
obj.on('drop', function (e) {

    $(this).css('border', '2px dotted #CCC');
    e.preventDefault();
    var files = e.originalEvent.dataTransfer.files;
 
    //We need to send dropped files to Server
    handleFileUpload(files, obj);
});

$(document).on('dragenter', function (e) {
    e.stopPropagation();
    e.preventDefault();
});
$(document).on('dragover', function (e) {
    e.stopPropagation();
    e.preventDefault();
    obj.css('border', '2px dotted #CCC');
});
$(document).on('drop', function (e) {
    e.stopPropagation();
    e.preventDefault();
});

function handleFileUpload(files, obj) {
    for (var i = 0; i < files.length; i++) {
        var file = files[i];
        if (file.type.match('image.*')) {
            var fd = new FormData();
            fd.append(file.name, file);
            sendFileToServer(fd);
        }
    }
}

function sendFileToServer(formData) {
    $.ajax({
        type: "POST",
        url: "/Home/ConvertImage",
        contentType: false,
        processData: false,
        data: formData,
        success: function (result) {
            $('#myModal').modal('show')
            $("#DataText").text(result);
        }
    });
}

function ConvertCompleted(response) {
    $('#myModal').modal('show')
    $("#DataText").text(response);
}

$("#UploadImage").submit(function (event) {
    var data = new FormData();
    var selectedFiles = $('#FileUpload')[0].files;
    if (selectedFiles.length == 1) {
        var file = selectedFiles[0];
        if (file.type.match('image.*')) {
            data.append(file.name, file);
        }
    }

    $.ajax({
        type: "POST",
        url: "/Home/ConvertImage",
        contentType: false,
        processData: false,
        data: data,
        success: function (result) {
            $('#myModal').modal('show')
            $("#DataText").text(result);
        }
    });

    event.preventDefault();
});