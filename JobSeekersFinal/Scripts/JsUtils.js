function VerifyNewAccount() {
    var proposedEmail = $("#email_form").val(),
        proposedPassw = $("#password_form").val(),
        confirmPassW = $("#verify_password_form").val();

    if (proposedPassw != confirmPassW) {
        alert("Two entered passwords are not the same.  Please correct.");
        return;
    }

    var jsonData = {
        email: proposedEmail,
        password: proposedPassw
    };

    $.ajax({
        url: window.location.origin + "/Home/AddNewAccount",
        contentType: "application/json",
        data: JSON.stringify(jsonData),
        success: function (data) {
            if (data.Success) {
                window.location = window.location.origin + "/Home/NewApplicantProfile?email=" + proposedEmail;
            } else {
                alert(data.Message);
            }
        }
    });

}



function SendProfileData() {

    var jsonData = {
    firstName : $("#new_app_first_name").val(),
    middleName : $("#new_app_middle_name").val(),
    lastName : $("#new_app_last_name").val(),
    street : $("#new_app_street").val(),
    city : $("#new_app_city").val(),
    state : $("#new_app_state").val(),
    zip : $("#new_app_zip").val(),
    phone : $("#new_app_phone").val(),
    email : $("#new_app_email").val(),
    positions : $("#new_app_positions").val(),
    skills: $("#new_app_skills").val(),
    //createdate: $("#new_app_createdate").val(),
    }

    // Store for sending with the Personality Quiz results, to know who to save for.
    window.emailInProgress = jsonData.email;

    if (jsonData.firstName.length == 0 ||
        jsonData.lastName.length == 0 ||
        jsonData.middleName.length == 0 ||
        jsonData.street.length == 0 ||
        jsonData.city.length == 0 ||
        jsonData.state.length == 0 ||
        jsonData.zip.length == 0 ||
        jsonData.phone.length == 0 ||
        jsonData.email.length == 0 ||
        jsonData.positions.length == 0) {
        alert("Please fill in all fields with data before proceeding.");
        return;
    }

    $.ajax({
        url: window.location.origin + "/Home/Personality1",
        contentType: "application/html",
        data: JSON.stringify(jsonData),
        success: function (result) {
            $("#form_container").html(result);
        }
    });
}

function SubmitQuizSection() {
    
    var answers = {
        email: window.emailInProgress,
        answerArray: [
            $("[name='Q1']").val(),
            $("[name='Q2']").val(),
            $("[name='Q3']").val(),
            $("[name='Q4']").val(),
            $("[name='Q5']").val(),
            ]
    }

    if (answers.answerArray[0] == null ||
        answers.answerArray[1] == null ||
        answers.answerArray[2] == null ||
        answers.answerArray[3] == null ||
        answers.answerArray[4] == null) {
        alert("Please answer all questions before proceeding.");
        return;
    }

    var method = $("#next_page").text() == "0"
    ? window.location.origin + "/Home/ApplicationComplete"
    : window.location.origin + "/Home/Personality" + $("#next_page").text();

    $.ajax({
        url: method,
        contentType: "application/json",
        data: JSON.stringify(answers),
        success: function (result) {
            $("#form_container").html(result);
        }
    });
}