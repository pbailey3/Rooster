Dropzone.autoDiscover = false;

function ConfigureHideMore() {
    $('#accMoreInfoFileUp').accordion({
        heightStyle: "content",
        collapsible: true,
        active: false
    });
}

function ConfigureDropZone() {
    var previewNode = document.querySelector("#template");
    previewNode.id = "";
    var previewTemplate = previewNode.parentNode.innerHTML;
    previewNode.parentNode.removeChild(previewNode);

    ConfigureHideMore();
    $('#resultsFileUpload').hide();
    var newDropZone = new Dropzone("#UploadFileForm",
            {
                url: "/FileImport/FileUpload",
                uploadMultiple: false,
                autoProcessQueue: false,
                maxFiles: 1,
                acceptedFiles: ".csv",
                dictInvalidFileType: "File type not valid",
                dictMaxFilesExceeded: "Maximum number of files has been reached",
                dictDefaultMessage: "Drag or click here to upload a file",
                thumbnailWidth: 80,
                thumbnailHeight: 80,
                previewsContainer: "#previewsFileUpload",
                previewTemplate: previewTemplate,
                clickable: "#previewsFileUpload",
                init: function () {
                    var myDropzone = this;

                    this.element.querySelector("#UploadButtonSubmit").addEventListener("click", function (e) {
                        e.preventDefault();
                        e.stopPropagation();
                        myDropzone.processQueue();
                    });

                    this.on("success", function (file, response) {
                        CompleteUploaded(response);
                    });

                }
            }
        );
    newDropZone.on("maxfilesexceeded", function (file) {
        if (file.status != "removed" || (file.status == "error" && file.accepted == false)) {
            newDropZone.removeFile(file);
        }
    });

    newDropZone.on("complete", function (file) {
        if (file.status == "error" && file.accepted == false) {
            newDropZone.removeFile(file);
            file.status = "removed";
        }
    });

    newDropZone.on("addedfile", function (file) {
        console.log(file.status);
        if (newDropZone.files.length > 0) {
            $("#UploadFileForm").addClass("dz-started");
        }
    });

    newDropZone.on("queuecomplete", function (progress) {
        $(".progress").hide();
    });
}

function CompleteUploaded(response) {
    var src = "/Images/csv_FileUploadSuccess.jpg";

    if (response.Status == "OK") {
        var index;
        var text;

        if (response.ErrorLines > 0 || response.LoadedSuccesfully == 0) {
            var src = "/Images/csv_FileUploadFailed.jpg";
        }
        $("#previewsFileUpload #previewsImage").attr("src", src);
        $('#resultsFileUpload').show();
        $('#formFileUpload').hide();
        $('#resultsLinesRead').text(response.LinesRead);
        $('#resultsValidLines').text(response.ValidLines);
        $('#resultsInvalidLines').text(response.ErrorLines);
        $('#resultsLoadSuccess').text(response.LoadedSuccesfully);

        for (index = 0; index < response.ErrorList.length; index++) {
            text = "<p class='text-danger'><span class='glyphicon glyphicon-minus' aria-hidden='true'></span> " + response.ErrorList[index] + "</p>";
            $('#content-accMoreInfoFileUp').append(text);
        }
    } else if (response.Status == "Failed") {
        src = "~/Images/csv_FileUploadFailed.jpg";
        $("#previewsFileUpload #previewsImage").attr("src", src);
    }
}
