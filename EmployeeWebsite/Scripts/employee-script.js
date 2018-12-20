var name = document.getElementById("form-name");
var form = document.getElementById("creationform");

function onSubmitHandler(formId)
{ /*formId is creationForm */
    
    if (formId.includes("creationForm")) {
        console.log(name.value)
    }
}