function printComponent(componentSelector) {
    var elementToPrint = document.querySelector(componentSelector);
    if (elementToPrint) {
        var printWindow = window.open('', '_blank');
        printWindow.document.open();
        printWindow.document.write('<html><head><title>Print</title>' +
            '<style type="text/css">' +
            '@page { size: auto;' + 
            'margin-top: 0; margin-bottom: 0; } ' +
            'table th, table td {' +
            'border:1px solid #8C8C8C;padding:1px;' +
            '}' +
            '</style>' +
            '</head><body>');
        printWindow.document.write(elementToPrint.innerHTML);
        printWindow.document.write('</body></html>');
        printWindow.document.close();
        printWindow.print();
        printWindow.close();
    }
}
