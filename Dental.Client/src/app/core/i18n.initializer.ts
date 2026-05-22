import { inject, provideAppInitializer } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { firstValueFrom } from 'rxjs';

export type AppLang = 'en' | 'tr';

export function readStoredLang(): AppLang {
  if (typeof localStorage === 'undefined') {
    return 'tr';
  }

  const saved = localStorage.getItem('dental-lang');
  if (saved === 'en' || saved === 'tr') {
    return saved;
  }

  return 'tr';
}

/** Load saved language before the first route renders (avoids English flash / stale translate pipe cache). */
export function provideI18nInitializer() {
  return provideAppInitializer(() => {
    const translate = inject(TranslateService);
    return firstValueFrom(translate.use(readStoredLang()));
  });
}
