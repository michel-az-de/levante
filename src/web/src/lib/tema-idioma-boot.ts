// Script anti-FOUC injetado inline no <head> pela fatia de layout do (site): le a
// preferencia de tema/idioma do localStorage e ajusta os atributos no <html> ANTES
// do primeiro paint, evitando flash. Espelha as chaves que TemaToggle e
// IdiomaProvider escrevem. Roda como string inline (sem acesso ao bundle), entao
// e JS puro, minimo e defensivo.
export const scriptBootTemaIdioma = `(function(){try{
var d=document.documentElement;
var tema=localStorage.getItem("levante:tema");
if(tema==="light"||tema==="dark")d.setAttribute("data-theme",tema);
var idioma=localStorage.getItem("levante:idioma");
if(idioma==="pt"||idioma==="en")d.setAttribute("data-idioma",idioma);
}catch(e){}})();`;
