import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, Observable, throwError } from 'rxjs';
import { LocalStorageUtils } from '../utils/localstorage';
import { NotificacaoService } from '../utils/notificacao.service';

@Injectable({
  providedIn: 'root'
})
export class AuthInterceptor implements HttpInterceptor {

constructor(private router: Router,  private notificacao: NotificacaoService) { }
localStorageUtil = new LocalStorageUtils();

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
      catchError(error => {
      if (error instanceof HttpErrorResponse) {
          if (error.status === 500) {
            this.notificacao.show('Ops! Ocorreu um erro, tente novamente ou nos contate.', 'falha')
          }
          if (error.status === 401) {
              this.localStorageUtil.limparDadosLocaisUsuario();
              this.router.navigate(['/conta/login'], { queryParams: { returnUrl: this.router.routerState.snapshot.url}});
          }
      }
      return throwError(() => error);
  }));
  }

}
