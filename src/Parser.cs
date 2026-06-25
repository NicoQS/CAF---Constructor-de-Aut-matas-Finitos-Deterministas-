
using System;



public class Parser {
	public const int _EOF = 0;
	public const int _ident = 1;
	public const int _string = 2;
	public const int maxT = 17;

	const bool _T = true;
	const bool _x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

public TablaSimbolos tab;
	public AutomataBuilder builder;



	public Parser(Scanner scanner) {
		this.scanner = scanner;
		errors = new Errors();
	}

	void SynErr (int n) {
		if (errDist >= minErrDist) errors.SynErr(la.line, la.col, n);
		errDist = 0;
	}

	public void SemErr (string msg) {
		if (errDist >= minErrDist) errors.SemErr(t.line, t.col, msg);
		errDist = 0;
	}
	
	void Get () {
		for (;;) {
			t = la;
			la = scanner.Scan();
			if (la.kind <= maxT) { ++errDist; break; }

			la = t;
		}
	}
	
	void Expect (int n) {
		if (la.kind==n) Get(); else { SynErr(n); }
	}
	
	bool StartOf (int s) {
		return set[s, la.kind];
	}
	
	void ExpectWeak (int n, int follow) {
		if (la.kind == n) Get();
		else {
			SynErr(n);
			while (!StartOf(follow)) Get();
		}
	}


	bool WeakSeparator(int n, int syFol, int repFol) {
		int kind = la.kind;
		if (kind == n) {Get(); return true;}
		else if (StartOf(repFol)) {return false;}
		else {
			SynErr(n);
			while (!(set[syFol, kind] || set[repFol, kind] || set[0, kind])) {
				Get();
				kind = la.kind;
			}
			return StartOf(syFol);
		}
	}

	
	void Automata() {
		tab = new TablaSimbolos(); 
		builder = new AutomataBuilder(); 
		Expect(3);
		Expect(1);
		string nombreAutomata = t.val; 
		builder.IniciarAutomata(nombreAutomata); 
		Expect(4);
		Expect(5);
		ListaSimbolos();
		
		Expect(6);
		Expect(5);
		ListaEstados();
		
		Expect(7);
		Expect(5);
		Expect(1);
		builder.DefinirEstadoInicial(t.val); 
		if (la.kind == 8) {
			Get();
			Expect(5);
			ListaEstadosFinales();
			
		}
		Expect(9);
		Expect(5);
		while (la.kind == 1) {
			Transicion();
		}
		if (!builder.TieneTransiciones()) {
		   SemErr("ERROR: El autÃ³mata debe tener al menos una transiciÃ³n."); 
		} 
		Expect(10);
		if (errors.count == 0) {
		    builder.FinalizarConValidacion(); 
		} else {
		    builder.FinalizarSinVisualizacion();
		} 
	}

	void ListaSimbolos() {
		Expect(2);
		string simbolo1 = t.val.Trim('"', '\'');
		tab.AgregarSimbolo(simbolo1);
		builder.AgregarSimbolo(simbolo1); 
		while (la.kind == 11) {
			Get();
			Expect(2);
			string simbolo2 = t.val.Trim('"', '\''); 
			tab.AgregarSimbolo(simbolo2);
			builder.AgregarSimbolo(simbolo2); 
		}
	}

	void ListaEstados() {
		Expect(1);
		if (tab.EstadoExiste(t.val)) {
		  SemErr("ERROR: Estado duplicado: " + t.val);
		} else {
		  tab.AgregarEstado(t.val);
		  builder.AgregarEstado(t.val);
		} 
		while (la.kind == 11) {
			Get();
			Expect(1);
			if (tab.EstadoExiste(t.val)) {
			  SemErr("ERROR: Estado duplicado: " + t.val);
			} else {
			  tab.AgregarEstado(t.val);
			  builder.AgregarEstado(t.val);
			} 
		}
	}

	void ListaEstadosFinales() {
		Expect(1);
		if (!tab.EstadoExiste(t.val)) {
		  SemErr("ERROR: Estado final no declarado: " + t.val);
		} else {
		  builder.MarcarEstadoFinal(t.val);
		} 
		while (la.kind == 11) {
			Get();
			Expect(1);
			if (!tab.EstadoExiste(t.val)) {
			  SemErr("ERROR: Estado final no declarado: " + t.val);
			} else {
			  builder.MarcarEstadoFinal(t.val);
			} 
		}
	}

	void Transicion() {
		Expect(1);
		string estadoOrigen = t.val; 
		if (!tab.EstadoExiste(estadoOrigen)) {
		  SemErr("ERROR: Estado origen no declarado: " + estadoOrigen);
		} 
		if (la.kind == 12) {
			Get();
		} else if (la.kind == 13) {
			Get();
		} else SynErr(18);
		
		Expect(1);
		string estadoDestino = t.val; 
		if (!tab.EstadoExiste(estadoDestino)) {
		  SemErr("ERROR: Estado destino no declarado: " + estadoDestino);
		} 
		if (la.kind == 14) {
			Get();
		} else if (la.kind == 15) {
			Get();
		} else if (la.kind == 16) {
			Get();
		} else SynErr(19);
		
		Expect(2);
		string simboloTrans = t.val.Trim('"', '\''); 
		string clave = estadoOrigen + "-" + simboloTrans;
		if (tab.EstadoExiste(estadoOrigen) && tab.EstadoExiste(estadoDestino)) {
		if (tab.TransicionExiste(clave)) {
		   SemErr("ERROR: Transicion duplicada desde " + estadoOrigen + " con simbolo '" + simboloTrans + "'");
		} else {
		   tab.AgregarTransicion(clave, estadoDestino);
		   builder.AgregarTransicion(estadoOrigen, estadoDestino, simboloTrans);
		}
		} 
		while (la.kind == 11) {
			Get();
			Expect(2);
			string simboloTrans2 = t.val.Trim('"', '\''); 
			string clave2 = estadoOrigen + "-" + simboloTrans2;
			if (tab.EstadoExiste(estadoOrigen) && tab.EstadoExiste(estadoDestino)) {
			if (tab.TransicionExiste(clave2)) {
			 SemErr("ERROR: Transicion duplicada desde " + estadoOrigen + " con simbolo '" + simboloTrans2 + "'");
			} else {
			 tab.AgregarTransicion(clave2, estadoDestino);
			 builder.AgregarTransicion(estadoOrigen, estadoDestino, simboloTrans2);
			}
			} 
		}
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		Automata();
		Expect(0);

	}
	
	static readonly bool[,] set = {
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x}

	};
} // end Parser


public class Errors {
	public int count = 0;                                    // number of errors detected
	public System.IO.TextWriter errorStream = Console.Out;   // error messages go to this stream
	public string errMsgFormat = "-- line {0} col {1}: {2}"; // 0=line, 1=column, 2=text

	public virtual void SynErr (int line, int col, int n) {
		string s;
		switch (n) {
			case 0: s = "EOF expected"; break;
			case 1: s = "ident expected"; break;
			case 2: s = "string expected"; break;
			case 3: s = "\"AUTOMATA\" expected"; break;
			case 4: s = "\"ALFABETO\" expected"; break;
			case 5: s = "\":\" expected"; break;
			case 6: s = "\"ESTADOS\" expected"; break;
			case 7: s = "\"INICIAL\" expected"; break;
			case 8: s = "\"FINALES\" expected"; break;
			case 9: s = "\"TRANSICIONES\" expected"; break;
			case 10: s = "\"FIN\" expected"; break;
			case 11: s = "\",\" expected"; break;
			case 12: s = "\"->\" expected"; break;
			case 13: s = "\"\u00e2\u0086\u0092\" expected"; break;
			case 14: s = "\"con\" expected"; break;
			case 15: s = "\"mediante\" expected"; break;
			case 16: s = "\"usando\" expected"; break;
			case 17: s = "??? expected"; break;
			case 18: s = "invalid Transicion"; break;
			case 19: s = "invalid Transicion"; break;

			default: s = "error " + n; break;
		}
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}

	public virtual void SemErr (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}
	
	public virtual void SemErr (string s) {
		errorStream.WriteLine(s);
		count++;
	}
	
	public virtual void Warning (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
	}
	
	public virtual void Warning(string s) {
		errorStream.WriteLine(s);
	}
} // Errors


public class FatalError: Exception {
	public FatalError(string m): base(m) {}
}
