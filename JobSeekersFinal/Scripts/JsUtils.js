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
    FirstName : $("#new_app_first_name").val(),
    MiddleName : $("#new_app_middle_name").val(),
    LastName : $("#new_app_last_name").val(),
    Street : $("#new_app_street").val(),
    City : $("#new_app_city").val(),
    State : $("#new_app_state").val(),
    Zip : $("#new_app_zip").val(),
    Phone : $("#new_app_phone").val(),
    Email : $("#new_app_email").val(),
    Positions : $("#new_app_positions").val(),
    Skills: $("#new_app_skills").val(),
    //createdate: $("#new_app_createdate").val(),
    }

    // Store for sending with the Personality Quiz results, to know who to save for.
    window.emailInProgress = jsonData.Email;

    if (jsonData.FirstName.length == 0 ||
        jsonData.LastName.length == 0 ||
        jsonData.MiddleName.length == 0 ||
        jsonData.Street.length == 0 ||
        jsonData.City.length == 0 ||
        jsonData.State.length == 0 ||
        jsonData.Zip.length == 0 ||
        jsonData.Phone.length == 0 ||
        jsonData.Email.length == 0 ||
        jsonData.Positions.length == 0) {
        alert("Please fill in all fields with data before proceeding.");
        return;
    }

    $.ajax({
        url: window.location.origin + "/Home/Personality1",
        contentType: "application/json",
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

function ValidateLogin() {

    var info = {
        UserEmail: $("#email_form").val(),
        Password: $("#password_form").val(),
    }

    $.ajax({
        url: window.location.origin + "/Home/Login?email=" + info.UserEmail + "&password=" + info.Password,
        contentType: "application/json",
        success: function (result) {
            if (result.Result) {
                if (result.AuthType === 2) {
                    window.location = window.location.origin + "/Home/Dashboard?authType=" + result.AuthType;
                }
                else {
                    window.location = window.location.origin + "/Home/Dashboard?authType=" + result.AuthType + "&email=" + info.UserEmail;
                }
                
            }
            else {
                alert("Unable to verify your account.  Please check your information or create a new account.");
            }
        }
    });
}

function LoadProfile(email) {

    $.ajax({
        url: window.location.origin + "/Home/DashboardProfile?email=" + email,
        contentType: "application/text",
        success: function (data) {
            if (!data.Result) {
                alert("Failed to load profile data!");
                return;
            }

            $("#profile-title").text(data.Title);
            $("#profile-name").text(data.Name);
            $("#profile-address").text(data.Address);
            $("#profile-phone").text(data.Phone);
            $("#profile-skills").text(data.Skills);
            $("#profile-email").attr("href", "mailto:" + data.Email);
            $("#profile-power").text(data.Power);
            $("#profile-inspirational").text(data.Inspirational);
            $("#profile-balance").text(data.Balance);
            $("#profile-analytical").text(data.Analytical);                
        }
    })

}