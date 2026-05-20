import { inject, provideAppInitializer } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { firstValueFrom } from 'rxjs';

export type AppLang = 'en' | 'tr';

export function readStoredLang(): AppLang {
  if (typeof localStorage === 'undefined') {
    return 'en';
  }

  const saved = localStorage.getItem('dental-lang');
  return saved === 'tr' ? 'tr' : 'en';
}

/** Load saved language before the first route renders (avoids English flash / stale translate pipe cache). */
export function provideI18nInitializer() {
  return provideAppInitializer(() => {
    const translate = inject(TranslateService);
    return firstValueFrom(translate.use(readStoredLang()));
  });
}
