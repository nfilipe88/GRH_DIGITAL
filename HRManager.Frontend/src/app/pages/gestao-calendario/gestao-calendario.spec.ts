import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GestaoCalendario } from './gestao-calendario';

describe('GestaoCalendario', () => {
  let component: GestaoCalendario;
  let fixture: ComponentFixture<GestaoCalendario>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GestaoCalendario]
    })
    .compileComponents();

    fixture = TestBed.createComponent(GestaoCalendario);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
