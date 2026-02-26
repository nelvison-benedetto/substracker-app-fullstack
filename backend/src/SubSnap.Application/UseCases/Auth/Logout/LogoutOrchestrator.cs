using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSnap.Application.UseCases.Auth.Logout;

//DA USARE SOLO QUANDO HANDLER DIVENTA DIFFICILE
internal class LogoutOrchestrator
{
    /*
        lo usi solo se quando Handler(che attualmente lavora anche come orchestrator) inizia a mostrare difficoltà di coordination perche e.g.supera >40-60rows / il flow (di Handler.cs) diventa molto lungo.
    */
}
