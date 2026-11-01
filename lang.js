(function(){var k="pathrova-lang",l;try{l=localStorage.getItem(k)}catch(e){}if(!l)l=(navigator.language||"de").slice(0,2)==="de"?"de":"en";document.documentElement.lang=l;
window.toggleLang=function(){var n=document.documentElement.lang==="de"?"en":"de";document.documentElement.lang=n;try{localStorage.setItem(k,n)}catch(e){}};})();
