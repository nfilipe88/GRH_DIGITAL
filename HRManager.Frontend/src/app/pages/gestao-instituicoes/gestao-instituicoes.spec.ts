import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GestaoInstituicoes } from './gestao-instituicoes';

describe('GestaoInstituicoes', () => {
  let component: GestaoInstituicoes;
  let fixture: ComponentFixture<GestaoInstituicoes>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GestaoInstituicoes]
    })
    .compileComponents();

    fixture = TestBed.createComponent(GestaoInstituicoes);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
