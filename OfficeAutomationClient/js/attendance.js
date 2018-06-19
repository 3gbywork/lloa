calander.showCal = function (selectDate) {
    var is5Row = (rows <= 5 ? true : false);
    if (typeof (selectDate) == "undefined") {
        selectDate = date = makeCal.setTimeZero(new Date());
    }
    selectDate = makeCal.setTimeZero(selectDate);
    var cldCache = null;//月份为转换后的月
    var month = selectDate.getMonth() + 1;
    $('#month_num').text(month);
    $('#year_num').text(selectDate.getFullYear());
    var table = "<table> \
						<thead class='tablehead'> \
							<tr> \
								<td class='thead' style='color:#bc5016;'>日</td> \
								<td class='thead'>一</td> \
								<td class='thead'>二</td> \
								<td class='thead'>三</td> \
								<td class='thead'>四</td> \
								<td class='thead'>五</td> \
								<td class='thead' style='color:#bc5016;'>六</td> \
							</tr> \
						</thead> \
					</table> \
					<table id='cont' style='height:100%;'> \
						<tbody>";
    var index = 40;
    var ind = 0;
    var rst = window.external.GetAttendance(selectDate.format("yyyy-MM-dd"));
    var att = eval(rst);
    for (var i = 0; i < rows; i++) {
        var aWeek = "<tr style='height:" + (1 / rows).toPercent() + ";'>";
        for (var j = 0; j < 7; j++) {
            if (calData[i][j] != null) {
                var tdclass = "";
                if (calData[i][j].today == true) {
                    tdclass = 'today';
                }
                var numtype = "number";

                var isClickBlock = "";
                if (calData[i][j].today == false && calData[i][j].value.getTime() == selectDate.getTime()) {
                    isClickBlock = " block_click";
                }
                var aDay = "";
                if (is5Row)
                    aDay = "<td i=" + i + " j=" + j + " style='height:" + (1 / rows).toPercent() + ";' class='block block5 " + tdclass + isClickBlock + "'>";
                else
                    aDay = "<td i=" + i + " j=" + j + " style='height:" + (1 / rows).toPercent() + ";' class='block " + tdclass + isClickBlock + "'>";

                aDay += "<a class='block_content'" +
                    "data='" + (calData[i][j].value.format('MMdd')) + "' href=\"javascript:;\">";
                var dateDay = calData[i][j].date;

                var tmpmonth = calData[i][j].value.getMonth() + 1;
                var tmpday = calData[i][j].value.getDate() - 1;
                if (tmpmonth == month && att.length > tmpday) {
                    if (att[tmpday].Attend != null && att[tmpday].Attend != "") {
                        aDay += "<div class='status attendance' style=\"width:" + att[tmpday].Attend.length * 21 + "px;\">" + att[tmpday].Attend + "</div>";
                    }
                    if (att[tmpday].Holiday != null) {
                        if (att[tmpday].Holiday == true) {
                            aDay += "<div class='status rest'>休</div>";
                        }
                        else {
                            aDay += "<div class='status work'>班</div>";
                        }
                    }
                }

                aDay += "<div style='display:inline-block;position:absolute;" +
                    "top:50%;width:100%;margin-top:-22px;left:0;'>";
                if (calData[i][j].weekend && !(calData[i][j].after || calData[i][j].before)) {
                    aDay += "<div class='" + numtype + "' style='color:#ff4e00;'>" + dateDay + "</div>";
                } else {
                    if (calData[i][j].after || calData[i][j].before) {
                        aDay += "<div class='" + numtype + "' style='color:#c3c3c3;'>" + dateDay + "</div>";
                    } else {
                        aDay += "<div class='" + numtype + "'>" + dateDay + "</div>";
                    }
                }
                //var dataStr= "";
                cldCache = cacheMgr.getCld(calData[i][j].value.getFullYear(), calData[i][j].value.getMonth());
                var dayStr = "";
                var color = "style='color:#BC5016;'";
                var title = "";
                if (cldCache[dateDay - 1].lunarFestival != undefined && cldCache[dateDay - 1].lunarFestival != '') {
                    dayStr = cldCache[dateDay - 1].lunarFestival;
                } else if (cldCache[dateDay - 1].solarFestival != undefined && cldCache[dateDay - 1].solarFestival != '') {
                    dayStr = cldCache[dateDay - 1].solarFestival;
                } else if (cldCache[dateDay - 1].solarTerms != undefined && cldCache[dateDay - 1].solarTerms != '') {
                    dayStr = cldCache[dateDay - 1].solarTerms;
                } else {
                    dayStr = calData[i][j].china.l_day;
                    color = "";
                    title = "";
                }

                if (color != "") {
                    var dayTitle = "";
                    if (cldCache[dateDay - 1].lunarFestival != undefined && cldCache[dateDay - 1].lunarFestival != '') {
                        dayTitle = cldCache[dateDay - 1].lunarFestival;
                        title += dayTitle;
                    }
                    if (cldCache[dateDay - 1].solarFestival != undefined && cldCache[dateDay - 1].solarFestival != '') {
                        dayTitle = cldCache[dateDay - 1].solarFestival;
                        if (title.trim() != "") {
                            title += "| ";
                        }
                        title += dayTitle;
                    }
                    if (cldCache[dateDay - 1].solarTerms != undefined && cldCache[dateDay - 1].solarTerms != '') {
                        dayTitle = cldCache[dateDay - 1].solarTerms;
                        if (title.trim() != "") {
                            title += "| ";
                        }
                        title += dayTitle;
                    }
                }

                aDay += "<div class='lnumber' title='" + title + "' " + color + ">" + dayStr + "</div>";
                aDay += "</div><div class='tdhover'></div></a></td>";
                aWeek += aDay;
            } else {
                aWeek += "<td></td>";
            }
        }
        aWeek += "</tr>";
        table += aWeek;
    }
    table += "</tbody></table>";
    $('#mainCal').empty();
    $('#mainCal').append(table);

    makeCal.makeAction();

    //加载当月的数据
    //		loadMonthEvent(selectDate);
    changeStyle();
}

function showCalendar(selectDate) {
    calander.showCal(selectDate);
}