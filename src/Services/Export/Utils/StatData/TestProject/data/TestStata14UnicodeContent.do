
// do not edit this file with Notepad.exe

clear all
use "TestStata14UnicodeContent.dta", clear

assert `"`:data label'"'=="Тестовый набор данных"
assert c(filedate)=="13 Nov 2015 11:13"
assert c(N)==5000000
assert c(k)==2

// check variable names
quietly ds
assert r(varlist)=="numvar strvar"

// check variable types
confirm byte variable numvar
confirm string variable strvar

// check variable labels
assert `"`:variable label numvar'"'=="Тестовая числовая переменная"
assert `"`:variable label strvar'"'=="Тестовая строковая переменная"

//check variable formats
assert `"`:format numvar'"'=="%6.0f"
assert `"`:format strvar'"'=="%9s"

// check values of numvar
assert numvar[1]==1
assert numvar[2]==2
assert numvar[3]==1
assert numvar[4]==2
summarize numvar, meanonly
assert r(min)==1
assert r(max)==2
assert reldif(r(mean),1.5)<0.0000000001

// check values of V2
assert strvar[1]=="абвгдежз АБВГДЕЖЗ abcdefgh ABCDEFGH"
assert strvar[2]=="古城肇庆沿西江而建，坐落在两个风景秀丽的自然公园之间。是探寻喀斯特地貌、溶洞景观和古镇村落的绝佳去处。"

// test is now completed successfully, create the marker
display filewrite("MarkerContent.txt","ok")

exit, STATA clear

