function chkcontrol(j) {
    // Checking if at least one period button is selected. Or not.
    var total = 0;
    for (var i = 0; i < document.form1.ckb.length; i++) {
        if (document.form1.ckb[i].checked) {
            total = total + 1;
        }
        if (total > 3) {
            //document.form1.ckb[i].checked=;
            alert("Please Select only three")
            document.form1.ckb[j].checked = false;
            return false;
        }
    }
}